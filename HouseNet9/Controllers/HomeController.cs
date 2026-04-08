using Data.Data.HouseRentalData;
using Data.Enums;
using HouseNet9.Controllers.Abstract;
using HouseNet9.Data;
using HouseNet9.Models;
using HouseNet9.ViewModels;
using Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HouseNet9.Controllers
{

    public class HomeController : BaseController
    {

        private readonly IEmailService _emailService;
        public HomeController(ILoggerFactory loggerFactory, ApplicationDbContext context, IEmailService emailService)
        : base(context, loggerFactory)
        {
            _emailService = emailService;
        }



        [Authorize]
        public IActionResult Secret()
        {
            return Content("Zalogowany 👍");
        }

        public async Task<IActionResult> Index(int? id)
        {
            IQueryable<House> query = _context.Houses
                .Where(i => i.IsActive)
                .Include(h => h.GeneralInformation)
                    .ThenInclude(g => g.Image)
                .Include(h => h.DescriptionPages)
                    .ThenInclude(d => d.Images)
                .Include(h => h.Distances)
                    .ThenInclude(d => d.Image)
                .Include(h => h.Distances)
                    .ThenInclude(d => d.DistanceItems)
                .AsNoTracking();

            if (id.HasValue)
                query = query.Where(h => h.HouseId == id.Value);
            else
                query = query.OrderBy(h => h.HouseId);

            var house = await query.FirstOrDefaultAsync();

            if (house == null)
                return NotFound();

            foreach (var descPage in house.DescriptionPages)
            {
                descPage.Images = descPage.Images
                    .OrderBy(img => img.Order)
                    .ToList();
            }

            return View(house);
        }


        //public async Task<IActionResult> More(int id, int houseId)
        //{

        //    DescriptionPage? dp = await _context.DescriptionPages.Where(w => w.DescriptionPageId == id)
        //        .Include(i => i.Images)
        //        .Include(i => i.House)
        //        .FirstOrDefaultAsync();

        //    GeneralInformation? gi = await _context.GeneralInformation.Where(w => w.House.HouseId == houseId)
        //        .Include(i => i.Image)
        //        .FirstOrDefaultAsync();

        //    List<DetailedInformation> di = await _context.DetailedInformation.Where(w => w.House.HouseId == houseId)
        //        .Include(i => i.Image)
        //        .Include(i => i.DetailedInformationItems)
        //        .ToListAsync();

        //    ViewBag.Detailed = di;

        //    return View(dp);
        //}


        public async Task<IActionResult> More(int id, int houseId)
        {
            // Pobierz descriptionPage wraz z obrazami i flagami
            DescriptionPage? dp = await _context.DescriptionPages
                .Where(w => w.DescriptionPageId == id)
                .Include(i => i.Images)
                .Include(i => i.House)
                .FirstOrDefaultAsync();

            if (dp == null)
                return NotFound();

            // General Information tylko jeśli flaga jest włączona
            List<GeneralInformation> gi = new List<GeneralInformation>();
            //GeneralInformation? gi = null;
            if (dp.EnabledSections.HasFlag(SectionType.General))
            {
                gi = await _context.GeneralInformation
                    .Where(w => w.House.HouseId == houseId)
                    .Include(i => i.Image)
                    .ToListAsync();
            }

            // Detailed Information tylko jeśli flaga jest włączona
            List<DetailedInformation> di = new List<DetailedInformation>();
            if (dp.EnabledSections.HasFlag(SectionType.Detailed))
            {
                di = await _context.DetailedInformation
                    .Where(w => w.House.HouseId == houseId)
                    .Include(i => i.Image)
                    .Include(i => i.DetailedInformationItems)
                    .ToListAsync();
            }

            // Distance tylko jeśli flaga jest włączona
            List<Distance> distances = new List<Distance>();
            if (dp.EnabledSections.HasFlag(SectionType.Distance))
            {
                distances = await _context.Distances
                    .Where(w => w.House.HouseId == houseId)
                    .Include(d => d.Image)
                    .Include(d => d.DistanceItems)
                    .ToListAsync();
            }

            // Przekaz do ViewBag lub ViewModel
            ViewBag.General = gi;
            ViewBag.Detailed = di;
            ViewBag.Distances = distances;

            return View(dp);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendEmail(ContactFooterVM vm)
        {
            var model = vm.Form;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Uzupełnij poprawnie formularz!";
                return RedirectToAction("Index", "Home");
            }

            // Tworzymy treść maila
            var subject = $"Nowa wiadomość od {model.Name}";
            var body = $@"
            Imię: {model.Name} <br />
            Email: {model.Email} <br />
            Wiadomość: <br />
            {model.Message}
            ";

            try
            {
                // Wysyłamy maila 
                _emailService.SendEmailAsync("kontakt@twojadomena.pl", subject, body);

                TempData["Success"] = "Wiadomość wysłana!";
            }
            catch (Exception ex)
            {
                // Możesz zalogować błąd
                _logger.LogError(ex, "Błąd wysyłania maila z formularza kontaktowego.");
                TempData["Error"] = "Wystąpił błąd podczas wysyłania wiadomości.";
            }

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
