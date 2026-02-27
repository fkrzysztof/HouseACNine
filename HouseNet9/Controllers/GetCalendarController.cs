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
            //*************************************** Send email **************************************

            // Obliczamy zaliczkę (30%)
            var deposit = rentalHouse.ToPay * 0.3m;

            // Tworzymy model emaila z wszystkimi danymi
            var emailModel = new NewReservationEmailViewModel
            {
                // ------------------- Dane domu -------------------
                HouseName = house.Name,
                HouseLogoUrl = null,
                RentalRules = house.RentalRules,

                // ------------------- Dane rezerwacji -------------------
                From = rentalHouse.From,
                To = rentalHouse.To,
                TotalPrice = rentalHouse.ToPay,
                Deposit = deposit,
                DepositDueDate = DateTime.Now.AddDays(3),
                Remaining = rentalHouse.ToPay - deposit,
                RemainingDueDate = rentalHouse.From.AddDays(-7),
                CreatedAt = rentalHouse.CreationDate,

                // ------------------- Dane klienta -------------------
                ClientFullName = rentalClient.FullName,
                ClientEmail = rentalClient.Email,
                ClientPhone = rentalClient.Phone,
                ClientStreet = rentalClient.Street,
                ClientNumber = rentalClient.Number,
                ClientZIPCode = rentalClient.ZIPCode,
                ClientCity = rentalClient.City,
                ClientCountry = rentalClient.Country,

                // ------------------- Kontakty i adresy domu -------------------
                Contacts = house.Contacts?.Select(c => new ContactEmailModel
                {
                    Name = c.Name,
                    Phones = c.PhoneNumbers?.Select(p => p.Number).ToList() ?? new List<string>(),
                    Emails = c.EmailAddresses?.Select(e => e.Email).ToList() ?? new List<string>(),
                    Addresses = c.Addresses?.Select(a => $"{a.Street}, {a.PostalCode} {a.City}, {a.Country}").ToList() ?? new List<string>()
                }).ToList() ?? new List<ContactEmailModel>()
            };

            // ------------------- Zbieramy maile adminów / właścicieli -------------------
            var adminEmails = emailModel.Contacts
                .SelectMany(c => c.Emails)
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct()
                .ToList();

            // ------------------- Wysyłka do klienta -------------------
            var clientMailBody = await _razorRenderer
                .RenderViewToStringAsync("Email/NewReservationClient", emailModel);

            await _emailService.SendEmailAsync(
                rentalClient.Email,
                "Potwierdzenie rezerwacji",
                clientMailBody);

            // ------------------- Wysyłka do właścicieli / adminów -------------------
            var ownerMailBody = await _razorRenderer
                .RenderViewToStringAsync("Email/NewReservationOwner", emailModel);

            foreach (var email in adminEmails)
            {
                await _emailService.SendEmailAsync(
                    email,
                    "Nowa rezerwacja domu",
                    ownerMailBody);
            }

            // ------------------- Przekierowanie po zakończeniu -------------------
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



    }
}
