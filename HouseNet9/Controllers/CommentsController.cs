using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Helpers;
using HouseNet9.Models;
using HouseNet9.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> List(bool? approved, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Comments.AsQueryable();

            if (approved.HasValue)
                query = query.Where(c => c.IsApproved == approved.Value);

            var totalItems = await query.CountAsync();

            var comments = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.Approved = approved;

            return View(comments);
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
            return RedirectToAction("List", "Comments"); // np. lista komentarzy dla admina
        }


        // GET
        public IActionResult Add(int houseId, string reservationCode, string email)
        {
            var reservation = _context.RentalHouses
                .Include(i => i.RentalClient)
                .FirstOrDefault(r => r.ReservationNumber == reservationCode
                                  && r.RentalClient.Email == email);

            var model = new Comment
            {
                HouseId = houseId,
                ReservationCode = reservationCode,
                Email = email,
                AuthorName = reservation.RentalClient.Name,
                StayFrom = reservation.From
            };

            return View(model);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Comment model)
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

            var reservation = await _context.RentalHouses.Include(i => i.RentalClient).FirstOrDefaultAsync(f => f.ReservationNumber == model.ReservationCode);
            model.CountryCode = CountryHelper.GetCountryCode(reservation.RentalClient.Country);

            _context.Comments.Add(model);
            _context.SaveChanges();

            TempData["Success"] = "Dziękujemy za opinię!";
            return RedirectToAction("Thanks", "Comments");
        }



        //public IActionResult Verify(int houseId)
        //{
        //    ViewBag.HouseId = houseId;
        //    return View();
        //}


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        //public IActionResult Verify(int houseId, string reservationCode, string email)
        public IActionResult Index(string reservationCode, string email)
        {
            //var reservation = _context.RentalHouses.Include(i => i.RentalClient)
            //    .FirstOrDefault(r => r.ReservationNumber == reservationCode
            //                         && r.RentalClient.Email == email
            //                         && r.HouseId == houseId);
            var reservation = _context.RentalHouses.Include(i => i.RentalClient)
                .FirstOrDefault(r => r.ReservationNumber == reservationCode
                                     && r.RentalClient.Email == email);

            if (reservation == null)
            {
                ModelState.AddModelError("", "Nie znaleziono rezerwacji dla tego domu");
                //ViewBag.HouseId = houseId;
                ViewBag.HouseId = reservation.HouseId;
                return View();
            }

            var now = DateTime.Now;

            // Sprawdzenie, czy pobyt się rozpoczął
            //if (reservation.From > now)
            //{
            //    ModelState.AddModelError("", "Nie możesz jeszcze dodać opinii – pobyt jeszcze się nie rozpoczął.");
            //    ViewBag.HouseId = houseId;
            //    return View();
            //}

            // Sprawdzenie, czy komentarz mieści się w oknie 30 dni po zakończeniu pobytu
            //var endWindow = reservation.To.AddDays(30);
            //if (now > endWindow)
            //{
            //    ModelState.AddModelError("", "Nie możesz dodać opinii – okres na wystawienie komentarza minął.");
            //    ViewBag.HouseId = houseId;
            //    return View();
            //}

            return RedirectToAction("Add", new
            {
                houseId = reservation.HouseId,
                reservationCode = reservation.ReservationNumber,
                email = reservation.RentalClient.Email
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, bool? approved, int page = 1)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
                return NotFound();

            comment.IsApproved = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("List", new { approved, page });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, bool? approved, int page = 1)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
                return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("List", new { approved, page });
        }


    }


    
}
