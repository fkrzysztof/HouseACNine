using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Helpers;
using HouseNet9.Services;
using HouseNet9.ViewModels;
using Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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



        //http
        //CreateNewReservation Przygotowanie rezerwacji 1
        [HttpPost]
        public async Task<IActionResult> CreateNewReservation([FromBody] ReservationRequest request)
        {
            var from = request.From.Date;
            var to = request.To.Date;
            var houseId = request.HouseId;

            if (await _collisionService.HasCollisionAsync(houseId, from, to))
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



        //get
        //Create Wyświetlenie Formularza Klienta i przycisku do zapisania rezerwaji 2
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



        //http
        //Create ZAPIS NOWEJ REZERWACJI 3 ************** END ********************
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

            //zapisanie nowego klienta
            _context.Add(rentalClient);
            await _context.SaveChangesAsync();

            var rentalHouse = new RentalHouse
            {
                HouseId = houseId,
                From = from,
                To = to,
                RentalClientId = rentalClient.RentalClientId,
                RentalStatusID = 5, // Do zapłaty
                CreationDate = DateTime.Now,
                IsActive = true
            };

            rentalHouse.ToPay = await _calculator.CalculatePriceAsync(rentalHouse);

            _context.Add(rentalHouse);
            await _context.SaveChangesAsync();

            // pobranie domu z kontaktami do miala
            var house = await _context.Houses
                .Include(h => h.Contacts)
                    .ThenInclude(c => c.EmailAddresses)
                .FirstOrDefaultAsync(h => h.HouseId == houseId);

            if (house == null)
                return NotFound();

            //***************************************Send email**************************************

            // zbieramy maile adminów
            var adminEmails = house?.Contacts?
                .SelectMany(c => c.EmailAddresses)
                .Select(e => e.Email)
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct()
                .ToList();

            var deposit = rentalHouse.ToPay * 0.3m;

            var emailModel = new NewReservationEmailViewModel
            {
                HouseName = house.Name,
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


            //mail do klienta
            var clientMailBody = await _razorRenderer
                                .RenderViewToStringAsync("Email/NewReservationClient", emailModel);

            await _emailService.SendEmailAsync(
                rentalClient.Email,
                "Potwierdzenie rezerwacji",
                clientMailBody);


            //do admina
            var ownerEmailModel = new NewReservationOwnerEmailViewModel
            {
                HouseName = house.Name,
                From = rentalHouse.From,
                To = rentalHouse.To,
                TotalPrice = rentalHouse.ToPay,
                ClientName = rentalClient.FullName,
                ClientEmail = rentalClient.Email,
                ClientPhone = rentalClient.Phone,
                CreatedAt = rentalHouse.CreationDate
            };

            var ownerMailBody = await _razorRenderer
                               .RenderViewToStringAsync("Email/NewReservationOwner", ownerEmailModel);

            foreach (var email in adminEmails)
            {
                await _emailService.SendEmailAsync(
                    email,
                    "Nowa rezerwacja domu",
                    ownerMailBody);
            }


            //var mailBody = await _razorRenderer
            //    .RenderViewToStringAsync("Email/NewReservation", emailModel);



            //// wysyłka do klienta
            //await _emailService.SendEmailAsync(
            //    rentalClient.Email,
            //    "Potwierdzenie rezerwacji",
            //    mailBody);

            //// wysyłka do adminów
            //if (adminEmails != null && adminEmails.Any())
            //{
            //    foreach (var email in adminEmails)
            //    {
            //        await _emailService.SendEmailAsync(
            //            email,
            //            "Nowa rezerwacja domu",
            //            mailBody);
            //    }
            //}

            return RedirectToAction("ThanksForTheReservation");
        }



        //get
        //ThanksForTheReservation podziekowanie/potwierdzenie/instrukcja
        public IActionResult ThanksForTheReservation(RentalHouse rentalHouse)
        {

            return View(rentalHouse);
        }



        //CreateReservationRequest
        public class ReservationRequest
        {
            public int HouseId { get; set; }
            public DateTime From { get; set; }
            public DateTime To { get; set; }
        }



        //metoda pomocnicza do stylowania emaila
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

            return View("~/Views/Email/NewReservation.cshtml", model);
        }


    }
}
