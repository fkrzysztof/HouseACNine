using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Models;
using HouseNet9.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers
{

    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> LoadMore(int houseId, int skip = 0, int take = 6)
        {
            var total = await _context.Comments
                .Where(c => c.HouseId == houseId && c.IsApproved)
                .CountAsync();

            var comments = await _context.Comments
                .Where(c => c.HouseId == houseId && c.IsApproved)
                .OrderByDescending(c => c.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            ViewBag.TotalComments = total;

            return PartialView("_CommentsList", comments);
        }


        // GET: /Comments/Moderate/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Moderate(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            var model = new CommentModerationViewModel
            {
                Id = comment.Id,
                ClientText = comment.Text,
                AdminText = comment.AdminText,
                IsApproved = comment.IsApproved
            };

            return View(model);
        }

        // POST: /Comments/Moderate/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Moderate(CommentModerationViewModel model)
        {
            var comment = await _context.Comments.FindAsync(model.Id);
            if (comment == null) return NotFound();

            comment.IsApproved = model.IsApproved;
            comment.AdminText = model.AdminText; // dopisany komentarz admina

            await _context.SaveChangesAsync();

            TempData["Success"] = "Komentarz został zaktualizowany.";
            return RedirectToAction("Index", "Admin"); // np. lista komentarzy dla admina
        }


        // GET
        public IActionResult Add(int id)
        {
            var reservation = _context.RentalHouses.Include(i => i.RentalClient).FirstOrDefault(f => f.RentalHouseID == id);

            if (reservation == null)
                return NotFound();

            var model = new Comment
            {
                ReservationCode = reservation.ReservationNumber,
                Email = reservation.RentalClient.Email,
                HouseId = (int)reservation.HouseId,
                StayFrom = reservation.From,
                AuthorName = reservation.RentalClient.Name // jeśli masz
            };

            return View(model);
        }

        // POST
        [HttpPost]
        public IActionResult Add(Comment model)
        {
            // Sprawdzenie, czy komentarz dla tej rezerwacji już istnieje
            var exists = _context.Comments
                .Any(c => c.ReservationCode == model.ReservationCode);

            if (exists)
            {
                ModelState.AddModelError("", "Opinia już została dodana");
                return View(model);
            }

            // Ustawienie IsApproved domyślnie
            model.IsApproved = model.Rating > 2; // tylko komentarze z >2 gwiazdkami są automatycznie zatwierdzone
            model.CreatedAt = DateTime.Now;

            _context.Comments.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Dziękujemy za opinię!";
            return RedirectToAction("Thanks");
        }



        public IActionResult Verify(int houseId)
        {
            ViewBag.HouseId = houseId;
            return View();
        }

        [HttpPost]
        public IActionResult Verify(int houseId, string reservationCode, string email)
        {
            var reservation = _context.RentalHouses.Include(i => i.RentalClient)
                .FirstOrDefault(r => r.ReservationNumber == reservationCode
                                     && r.RentalClient.Email == email
                                     && r.HouseId == houseId);

            if (reservation == null)
            {
                ModelState.AddModelError("", "Nie znaleziono rezerwacji dla tego domu");
                ViewBag.HouseId = houseId;
                return View();
            }

            var now = DateTime.Now;

            // Sprawdzenie, czy pobyt się rozpoczął
            if (reservation.From > now)
            {
                ModelState.AddModelError("", "Nie możesz jeszcze dodać opinii – pobyt jeszcze się nie rozpoczął.");
                ViewBag.HouseId = houseId;
                return View();
            }

            // Sprawdzenie, czy komentarz mieści się w oknie 30 dni po zakończeniu pobytu
            var endWindow = reservation.To.AddDays(30);
            if (now > endWindow)
            {
                ModelState.AddModelError("", "Nie możesz dodać opinii – okres na wystawienie komentarza minął.");
                ViewBag.HouseId = houseId;
                return View();
            }

            return RedirectToAction("Add", new { id = reservation.RentalHouseID });
        }

    }


    
}
