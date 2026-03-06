using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Helpers;
using HouseNet9.Services;
using HouseNet9.Services.Payments;
using HouseNet9.ViewModels;
using Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Configuration;

namespace HouseRent.Controllers
{
    public class GetCalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly RentalCalculatorService _calculator;
        private readonly RentalCollisionService _collisionService;
        private readonly IRazorViewToStringRenderer _razorRenderer;
        private readonly IPaymentCalculator _paymentCalculator;

        public GetCalendarController(ApplicationDbContext context, IEmailService emailService, RentalCalculatorService calculator, 
            RentalCollisionService collisionService, IRazorViewToStringRenderer razorRenderer,IPaymentCalculator paymentCalculator)
        {
            _context = context;
            _emailService = emailService;
            _calculator = calculator;
            _collisionService = collisionService;
            _razorRenderer = razorRenderer;
            _paymentCalculator = paymentCalculator;
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
                RentalClientId = null //-1  // tymczasowy klient do obliczeń
            };

            tempRental.ToPay = await _calculator.CalculatePriceAsync(tempRental, true);


            var house = await _context.Houses
                              //.Include(h => h.Contacts)
                              //.ThenInclude(c => c.EmailAddresses)
                              .FirstOrDefaultAsync(h => h.HouseId == houseId);

            if (house == null)
                return NotFound();

            //sprawdzam settings, jak nie ma ustawionego to znaczy ze uzywa domyślnych ustawien.
            var settings = house.HouseSettingsId != null ? await _context.HouseSettings.FirstOrDefaultAsync(s => s.Id == house.HouseSettingsId)
                                                         : await _context.HouseSettings.FirstOrDefaultAsync(s => s.IsDefault);

            // Obliczamy zaliczkę (30%)
            //var deposit = rentalHouse.ToPay * 0.3m;
            PaymentCalculationResult payment = _paymentCalculator.Calculate(
                        tempRental.ToPay,
                        tempRental.From,
                        settings
                    );


            ViewBag.NewRentalInfo = tempRental;
            ViewBag.NewRentalInfoPayment = payment;
            ViewBag.SettingsInfo = settings;

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

            //var rentalHouse = new RentalHouse
            //{
            //    HouseId = houseId,
            //    From = from,
            //    To = to,
            //    RentalClientId = rentalClient.RentalClientId,
            //    RentalStatusID = 5, // Do zapłaty
            //    CreationDate = DateTime.Now,
            //    IsActive = true
            //};

            ////naliczanie aktualnej ceny całkowitej wynajmu
            //rentalHouse.ToPay = await _calculator.CalculatePriceAsync(rentalHouse);

            //_context.Add(rentalHouse);
            //await _context.SaveChangesAsync();

            // pobranie domu z kontaktami do miala
            //var house = await _context.Houses
            //    .Include(h => h.Contacts)
            //        .ThenInclude(c => c.EmailAddresses)
            //    .FirstOrDefaultAsync(h => h.HouseId == houseId);

            //if (house == null)
            //    return NotFound();
            //*************************************** Send email **************************************

            ////sprawdzam settings, jak nie ma ustawionego to znaczy ze uzywa domyślnych ustawien.
            //var settings = house.HouseSettingsId != null ? await _context.HouseSettings.FirstOrDefaultAsync(s => s.Id == house.HouseSettingsId)
            //                                             : await _context.HouseSettings.FirstOrDefaultAsync(s => s.IsDefault);

            // Obliczamy zaliczkę (30%)

            //var payment = _paymentCalculator.Calculate(
            //            rentalHouse.ToPay,
            //            rentalHouse.From,
            //            settings
            //        );


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

            // naliczanie ceny całkowitej
            rentalHouse.ToPay = await _calculator.CalculatePriceAsync(rentalHouse);

            // pobranie settings
            var house = await _context.Houses
                .Include(h => h.Contacts)
                    .ThenInclude(c => c.EmailAddresses)
                .FirstOrDefaultAsync(h => h.HouseId == houseId);

            if (house == null)
                return NotFound();

            var settings = house.HouseSettingsId != null
                ? await _context.HouseSettings.FirstOrDefaultAsync(s => s.Id == house.HouseSettingsId)
                : await _context.HouseSettings.FirstOrDefaultAsync(s => s.IsDefault);

            if (settings == null)
                throw new Exception("Brak konfiguracji płatności.");

            // obliczenie płatności
            var payment = _paymentCalculator.Calculate(
                rentalHouse.ToPay,
                rentalHouse.From,
                settings
            );

            // 🔴 TU ZAPISUJESZ POLA DO BAZY
            rentalHouse.DepositAmount = payment.Deposit;
            rentalHouse.DepositDueDate = payment.DepositDueDate;
            rentalHouse.RemainingAmount = payment.Remaining;
            rentalHouse.RemainingDueDate = payment.RemainingDueDate;

            _context.Add(rentalHouse);
            await _context.SaveChangesAsync();


            // Tworzymy model emaila z wszystkimi danymi
            var emailModel = new NewReservationEmailViewModel
            {
                // ------------------- Dane domu -------------------
                HouseName = house.Name,
                HouseLogoUrl = string.IsNullOrEmpty(settings.LogoFileName)  ? "/images/no_photography_24dp.svg" : "/uploads/" + settings.LogoFileName,
                RentalRules = house.RentalRules,

                // ------------------- Dane rezerwacji -------------------
                From = rentalHouse.From,
                To = rentalHouse.To,
                TotalPrice = payment.Total,
                Deposit = payment.Deposit,
                DepositDueDate = payment.DepositDueDate,
                //pozostala kwota
                Remaining = payment.Remaining,
                RemainingDueDate = payment.RemainingDueDate,
                CreatedAt = rentalHouse.CreationDate,
                Currency = settings.Currency ?? "€",
                DepositPercentage = settings.DepositPercentage,

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
            return RedirectToAction("ThanksForTheReservation", new
            {
                id = rentalHouse.RentalHouseID
            });

        }



        // --------------------------------------------------------
        // GET: Rental/ThanksForTheReservation/5
        // --------------------------------------------------------
        public async Task<IActionResult> ThanksForTheReservation(int id)
        {
            var rental = await _context.RentalHouses
                .Include(r => r.RentalClient)
                .FirstOrDefaultAsync(f => f.RentalHouseID == id);

            if (rental == null)
                return NotFound();

            var house = await _context.Houses
                .Include(h => h.Contacts)
                    .ThenInclude(c => c.EmailAddresses)
                .FirstOrDefaultAsync(h => h.HouseId == rental.HouseId);

            if (house == null)
                return NotFound();

            HouseSettings? settings = null;

            if (house.HouseSettingsId != null)
            {
                settings = await _context.HouseSettings
                    .FirstOrDefaultAsync(s => s.Id == house.HouseSettingsId);
            }

            if (settings == null)
            {
                settings = await _context.HouseSettings
                    .FirstOrDefaultAsync(s => s.IsDefault);
            }

            if (settings == null)
            {
                // awaryjne ustawienia żeby widok nie padł
                settings = new HouseSettings
                {
                    Currency = "PLN",
                    DepositPercentage = 30
                };
            }

            var paymentInfo = _paymentCalculator.Calculate(
                rental.ToPay,
                rental.From,
                settings
            );

            ViewBag.RentalInfo = rental;
            ViewBag.PaymentInfo = paymentInfo;
            ViewBag.SettingsInfo = settings;

            return View(rental.RentalClient);
        }
    }


    //CreateReservationRequest
    public class ReservationRequest
    {
        public int HouseId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }




}
