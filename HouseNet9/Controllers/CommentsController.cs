using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Helpers;
using HouseNet9.Models;
using HouseNet9.Services;
using HouseNet9.ViewModels;
using Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HouseNet9.Controllers
{

    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICommentNotificationService _commentNotificationService;

        public CommentsController(ApplicationDbContext context, ICommentNotificationService commentNotificationService)
        {
            _context = context;
            _commentNotificationService = commentNotificationService;
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


        [HttpGet]
        public async Task<IActionResult> Add(int houseId, string reservationCode, string email)
        {
            var reservation = await _context.RentalHouses
                .Include(i => i.RentalClient)
                .FirstOrDefaultAsync(r =>
                    r.ReservationNumber == reservationCode &&
                    r.RentalClient.Email == email);

            if (reservation == null)
                return NotFound();

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.ReservationCode == reservationCode);


            if (comment != null)
            {
                return View("AlreadyExists", new EditRequestViewModel
                {
                    ReservationCode = reservationCode,
                    Email = email
                });
            }

            var model = new Comment
            {
                HouseId = houseId,
                ReservationCode = reservationCode,
                Email = email,
                AuthorName = reservation.RentalClient.Name,
                StayFrom = reservation.From
            };

            return View("CommentForm", model);

        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Comment model)
        {
            //walidacja ********
            if (!ModelState.IsValid)
            {
                return View("CommentForm", model);
            }

            if (model.Rating < 1 || model.Rating > 4)
            {
                ModelState.AddModelError("Rating", "Ocena musi być od 1 do 4");
                return View("CommentForm", model);
            }
            //*********


            var reservation = await _context.RentalHouses
                .Include(i => i.RentalClient)
                .FirstOrDefaultAsync(f =>
                    f.ReservationNumber == model.ReservationCode &&
                    f.RentalClient.Email == model.Email);

            if (reservation == null)
                return NotFound();

            var existing = await _context.Comments
                .FirstOrDefaultAsync(c => c.ReservationCode == model.ReservationCode);

            //EXISTING
            if (existing != null)
            {
                return RedirectToAction("EditRequest");
            }

            // CREATE
            model.IsApproved = model.Rating > 2;
            model.CreatedAt = DateTime.Now;
            model.HouseId = (int)reservation.HouseId;
            model.CountryCode = CountryHelper.GetCountryCode(reservation.RentalClient.Country);
            model.AuthorName = reservation.RentalClient.Name;
            model.StayFrom = reservation.From;

            _context.Comments.Add(model);
            await _context.SaveChangesAsync();

            TempData["Title"] = "Dziękujemy za opinię! ⭐";
            TempData["Message"] = "Twoja opinia została zapisana i pomoże innym gościom.";
            return RedirectToAction("Thanks");
        }


       
        
        //Edycja komentarza z linka (token!)
        [HttpGet]
        public async Task<IActionResult> Edit(string token)
        {
            var access = await _context.CommentAccessTokens
                .Include(a => a.Comment)
                .FirstOrDefaultAsync(a => a.Token == token);

            if (access == null)
            {
                ViewBag.Reason = "not_found";
                return View("Expired");
            }

            if (access.IsUsed)
            {
                ViewBag.Reason = "used";
                return View("Expired");
            }

            if (access.ExpiresAt < DateTime.Now)
            {
                ViewBag.Reason = "token_expired";
                return View("Expired");
            }

            ViewBag.Token = token;
            ViewBag.CanEdit = access.Comment.CreatedAt >= DateTime.Now.AddDays(-14);

            var comment = access.Comment;

            ViewBag.IsEdit = true;
            return View("CommentForm", comment);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Comment model, string token)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = true;
                ViewBag.Token = token;
                return View("CommentForm", model);
            }

            var access = await _context.CommentAccessTokens
                .Include(a => a.Comment)
                .FirstOrDefaultAsync(a => a.Token == token);

            //sprawdzam tokena i komentarz
            if (access == null)
            {
                ViewBag.Reason = "not_found";
                return View("Expired");
            }

            if (access.IsUsed)
            {
                ViewBag.Reason = "used";
                return View("Expired");
            }

            if (access.ExpiresAt < DateTime.Now)
            {
                ViewBag.Reason = "token_expired";
                return View("Expired");
            }

            if (access.Comment.CreatedAt < DateTime.Now.AddDays(-14))
            {
                ViewBag.Reason = "edit_time_expired";
                return View("Expired");
            }
            //**************************

            var comment = access.Comment;

            if (comment.Id != access.CommentId)
                return BadRequest();

            if (model.Rating < 1 || model.Rating > 4)
            {
                ModelState.AddModelError("Rating", "Ocena musi być od 1 do 4");

                ViewBag.IsEdit = true;
                ViewBag.Token = token;
                return View("CommentForm", model);
            }

            comment.Text = model.Text;
            comment.Rating = model.Rating;

            access.IsUsed = true;

            await _context.SaveChangesAsync();

            TempData["Title"] = "Zapisano zmiany!";
            TempData["Message"] = "Twoja opinia została zaktualizowana.";
            return RedirectToAction("Thanks");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRequest(string reservationCode, string email)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c =>
                    c.ReservationCode == reservationCode &&
                    c.Email == email);

            if (comment == null)
            {
                ModelState.AddModelError("", "Nie znaleziono opinii.");
                return View();
            }

            var oldTokens = await _context.CommentAccessTokens
                .Where(t => t.CommentId == comment.Id && !t.IsUsed)
                .ToListAsync();

            oldTokens.ForEach(t => t.IsUsed = true);

            // generowanie tokena
            var token = Guid.NewGuid().ToString();

            var access = new CommentAccessToken
            {
                Token = token,
                CommentId = comment.Id,
                ExpiresAt = DateTime.Now.AddHours(24),
                IsUsed = false
            };

            _context.CommentAccessTokens.Add(access);
            await _context.SaveChangesAsync();

            //email
            await _commentNotificationService.SendCommentEditLinkAsync(comment, token);

            TempData["Title"] = "Link wysłany";
            TempData["Message"] = "Wysłaliśmy link do edycji na Twój email.";
            return RedirectToAction("Thanks");
        }



        public IActionResult Thanks()
        {
            return View();
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string reservationCode, string email)
        {

            var reservation = _context.RentalHouses.Include(i => i.RentalClient)
                .FirstOrDefault(r => r.ReservationNumber == reservationCode
                                     && r.RentalClient.Email == email);
            

            if (reservation == null)
            {
                ModelState.AddModelError("", "Nie znaleziono rezerwacji dla tego domu");
                return View();
            }

            var now = DateTime.Now;

            // Sprawdzenie, czy pobyt się rozpoczął
            if (reservation.From > now)
            {
                ModelState.AddModelError("", "Nie możesz jeszcze dodać opinii – pobyt jeszcze się nie rozpoczął.");
                //ViewBag.HouseId = houseId;
                return View();
            }

            // Sprawdzenie, czy komentarz mieści się w oknie 30 dni po zakończeniu pobytu
            var endWindow = reservation.To.AddDays(30);
            if (now > endWindow)
            {
                ModelState.AddModelError("", "Nie możesz dodać opinii – okres na wystawienie komentarza minął.");
                //ViewBag.HouseId = houseId;
                return View();
            }

            return RedirectToAction("Add", new
            {
                houseId = reservation.HouseId,
                reservationCode = reservation.ReservationNumber,
                email = reservation.RentalClient.Email
            });
        }

        //Szybkie zatwierdzenie / cofnięcie zatwierdzenia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, bool? approved, int page = 1)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
                return NotFound();

            comment.IsApproved = approved ?? true;

            await _context.SaveChangesAsync();

            return RedirectToAction("List", new { approved, page });
        }



        //Usuwa komentarz
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


        public IActionResult PreviewEmail()
        {
            var model = new EmailSimpleViewModel
            {
                Title = "Nowy komentarz",
                LogoUrl = "logo.png", // wrzuć coś do /uploads/
                ButtonUrl = "https://twojastrona.pl/comment?token=FAKE_TOKEN_123",
                ButtonText = "Zobacz komentarz",
                FooterText = "To jest podgląd wiadomości email."
            };

            return View("/Views/Emails/CommentNotification.cshtml", model);
        }


    }


    
}
