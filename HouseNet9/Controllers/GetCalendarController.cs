using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Helpers;
using HouseNet9.Services;
using HouseNet9.ViewModels;
using Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

namespace HouseRent.Controllers
{
    public class GetCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly RentalCalculatorService _calculator;
        private readonly RentalCollisionService _collisionService;
        private readonly IRazorViewToStringRenderer _razorRenderer;

        public GetCalendarController(ApplicationDbContext context, IEmailService emailService, RentalCalculatorService calculator, 
            RentalCollisionService collisionService, IRazorViewToStringRenderer razorRenderer)
        {
            _context = context;
            _emailService = emailService;
            _calculator = calculator;
            _collisionService = collisionService;
            _razorRenderer = razorRenderer;
        }

        // Pobiera zajęte dni w zadanym zakresie
        //[HttpGet("reserved")]
        public async Task<IActionResult> GetReservedDates(DateTime start, DateTime end)
        {
            var reservations = await _context.RentalHouses
                .Where(r => r.From <= end && r.To >= start)
                .Select(r => new { r.From, r.To })
                .ToListAsync();

            var reservedDates = new List<string>();
            foreach (var r in reservations)
            {
                var s = r.From < start ? start : r.From;
                var e = r.To > end ? end : r.To;
                for (var d = s.Date; d <= e.Date; d = d.AddDays(1))
                {
                    reservedDates.Add(d.ToString("yyyy-MM-dd"));
                }
            }

            return Ok(reservedDates.Distinct());
        }


        
        //JS ACTION
        // POST: GetCalendar/Info
        [HttpPost]
        public async Task<IActionResult> Info([Bind("From,HouseId,HowManyDaysFromSelect")]  RentalHouse rentalHouse)
        {

                rentalHouse.To = rentalHouse.From.AddDays(rentalHouse.HowManyDaysFromSelect);
                rentalHouse.CreationDate = DateTime.Now;
                rentalHouse.IsActive = true;

                RentalPrice? rentalPrice = new RentalPrice();
                rentalPrice = await _context.RentalPrices.FirstOrDefaultAsync(f => f.HouseId == rentalHouse.HouseId);

                if (rentalPrice != null)
                {
                    if (rentalHouse.HowManyDaysFromSelect == 13)
                        rentalHouse.ToPay = rentalHouse.HowManyDaysFromSelect * rentalPrice.TwoWeeks;
                    if (rentalHouse.HowManyDaysFromSelect == 9)
                        rentalHouse.ToPay = rentalHouse.HowManyDaysFromSelect * rentalPrice.OneWeek;
                    if (rentalHouse.HowManyDaysFromSelect == 6)
                        rentalHouse.ToPay = rentalHouse.HowManyDaysFromSelect * rentalPrice.OneWeek;
                }

            HttpContext.Session.SetString("Rental", JsonConvert.SerializeObject(rentalHouse));
                
            ViewBag.NewRentalInfo = rentalHouse;
                return PartialView();
            }

        public IActionResult ThanksForTheReservation(RentalHouse rentalHouse)
        {

            return View(rentalHouse);
        }

        public async Task<IActionResult> Create()
        {
            // Pobranie danych z TempData (wybrany termin i dom)
            var (from, to, houseId) = ReservationHelper.GetReservationFromTempData(TempData);

            if (from == DateTime.MinValue || to == DateTime.MinValue || houseId == 0)
                return BadRequest("Brak danych rezerwacji");

            var tempRental = new RentalHouse
            {
                From = from,
                To = to,
                HouseId = houseId,
                RentalClientId = -1  // tymczasowy klient do obliczeń
            };

            tempRental.ToPay = await _calculator.CalculatePriceAsync(tempRental);

            ViewBag.NewRentalInfo = tempRental;

            return View();
        }


        public class ReservationViewModel
        {
            public DateTime From { get; set; }
            public DateTime To { get; set; }
            public int HouseId { get; set; }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalClient rentalClient, int houseId, DateTime from, DateTime to)
        {
            if (!ModelState.IsValid)
                return View(rentalClient);

            if (await _collisionService.HasCollisionAsync(houseId, from, to))
            {
                ModelState.AddModelError("", "Termin jest już zajęty.");
                return View(rentalClient);
            }

            var rentalHouse = new RentalHouse
            {
                HouseId = houseId,
                From = from,
                To = to,
                RentalClient = rentalClient,
                RentalStatusID = 5, // Do zapłaty
                CreationDate = DateTime.Now,
                IsActive = true
            };

            rentalHouse.ToPay = await _calculator.CalculatePriceAsync(rentalHouse);

            _context.Add(rentalHouse);
            await _context.SaveChangesAsync();

            //*************************************** Send email **************************************
            var deposit = rentalHouse.ToPay * 0.3m;

            var emailModel = new NewReservationEmailViewModel
            {
                HouseName = rentalHouse.House.Name,
                From = rentalHouse.From,
                To = rentalHouse.To,
                TotalPrice = rentalHouse.ToPay,
                Deposit = deposit,
                DepositDueDate = DateTime.Now.AddDays(3),
                Remaining = rentalHouse.ToPay - deposit,
                RemainingDueDate = rentalHouse.From.AddDays(-7),
                ClientName = rentalClient.FullName,
                ClientEmail = rentalClient.Email,
                ClientPhone = rentalClient.Phone,
                CreatedAt = rentalHouse.CreationDate
            };

            var mailBody = await _razorRenderer
                .RenderViewToStringAsync("/Views/Emails/NewReservation.cshtml", emailModel);


            // pobranie domu z kontaktami
            var house = await _context.Houses
                .Include(h => h.Contacts)
                    .ThenInclude(c => c.EmailAddresses)
                .FirstOrDefaultAsync(h => h.HouseId == rentalHouse.RentalHouseID);

            // zbieramy maile adminów
            var adminEmails = house?.Contacts?
                .SelectMany(c => c.EmailAddresses)
                .Select(e => e.Email)
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct()
                .ToList();

            // wysyłka do klienta
            await _emailService.SendEmailAsync(
                rentalClient.Email,
                "Potwierdzenie rezerwacji",
                mailBody);

            // wysyłka do adminów
            if (adminEmails != null && adminEmails.Any())
            {
                foreach (var email in adminEmails)
                {
                    await _emailService.SendEmailAsync(
                        email,
                        "Nowa rezerwacja domu",
                        mailBody);
                }
            }

            return RedirectToAction("ThanksForTheReservation");
        }



        public IActionResult DetailsInfo(DateTime start, DateTime end)
        {
            // Tutaj możesz pobrać szczegóły rezerwacji i przekazać do widoku
            ViewData["Start"] = start;
            ViewData["End"] = end;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewReservation([FromBody] ReservationRequest request)
        {
            var from = request.From.Date;
            var to = request.To.Date;
            var houseId = 1;

            if (await HasCollision(from, to))
                return Conflict();

            // Zapis do TempData jako string (bezpieczny format)
            TempData["From"] = from.ToString("yyyy-MM-dd");
            TempData["To"] = to.ToString("yyyy-MM-dd");
            TempData["HouseId"] = houseId.ToString();

            return Ok(new
            {
                success = true,
                redirectUrl = Url.Action("Create", "GetCalendar") // przekierowanie do Create
            });
        }





        //Walidacja kolizji
        private async Task<bool> HasCollision(DateTime from, DateTime to)
        {
            return await _context.RentalHouses
                .AnyAsync(r =>
                    r.From.Date <= to.Date &&
                    r.To.Date >= from.Date
                );
        }


        public class ReservationRequest
        {
            public DateTime From { get; set; }
            public DateTime To { get; set; }
        }

        public async Task<IActionResult> PreviewEmail()
        {
            var model = new NewReservationEmailViewModel
            {
                HouseName = "Dom testowy",
                From = DateTime.Today,
                To = DateTime.Today.AddDays(7),
                TotalPrice = 1200,
                Deposit = 360,
                DepositDueDate = DateTime.Today.AddDays(3),
                Remaining = 840,
                RemainingDueDate = DateTime.Today.AddDays(-7),
                ClientName = "Jan Kowalski",
                ClientEmail = "jan@test.pl",
                ClientPhone = "123 456 789",
                CreatedAt = DateTime.Now
            };

            return View("~/Views/Emails/NewReservation.cshtml", model);
        }


    }
}
