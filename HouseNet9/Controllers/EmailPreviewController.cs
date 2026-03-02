using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HouseNet9.Controllers
{
    public class EmailPreviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmailPreviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Podgląd dla klienta
        public async Task<IActionResult> PreviewClient()
        {
            var house = await _context.Houses
                .Include(h => h.Contacts!)
                    .ThenInclude(c => c.Addresses)
                .Include(h => h.Contacts!)
                    .ThenInclude(c => c.PhoneNumbers)
                .Include(h => h.Contacts!)
                    .ThenInclude(c => c.EmailAddresses)
                .Include(h => h.Settings) // <-- pobieramy ustawienia domu
                .FirstOrDefaultAsync(h => h.HouseId == 1);

            if (house == null)
                return NotFound("Nie znaleziono domu o ID 1.");

            //var settings = house.Settings ?? new HouseSettings(); // fallback do default
            //nie pokazuje logo
            var settings = house.HouseSettingsId != null ? await _context.HouseSettings.FirstOrDefaultAsync(s => s.Id == house.HouseSettingsId)
                                             : await _context.HouseSettings.FirstOrDefaultAsync(s => s.IsDefault);


            // Tworzymy PaymentCalculator
            var payment = new PaymentCalculator(
                totalPrice: 1200m,
                arrivalDate: DateTime.Today,
                settings: settings
            );

            // Tworzymy model email
            var model = new NewReservationEmailViewModel
            {
                // Dane domu
                HouseName = house.Name,
                HouseLogoUrl = settings.LogoFileName,
                RentalRules = house.RentalRules,
                Contacts = house.Contacts.Select(c => new ContactEmailModel
                {
                    Name = c.Name,
                    Phones = c.PhoneNumbers?.Select(p => p.Number).ToList() ?? new List<string>(),
                    Emails = c.EmailAddresses?.Select(e => e.Email).ToList() ?? new List<string>(),
                    Addresses = c.Addresses?.Select(a => $"{a.Street}, {a.PostalCode} {a.City}, {a.Country}").ToList() ?? new List<string>()
                }).ToList(),

                // Dane klienta
                ClientFullName = "Jan Kowalski",
                ClientEmail = "jan@test.pl",
                ClientPhone = "123 456 789",
                ClientStreet = "Przykładowa",
                ClientNumber = "1",
                ClientZIPCode = "00-000",
                ClientCity = "Warszawa",
                ClientCountry = "Polska",

                // Dane rezerwacji
                From = DateTime.Today,
                To = DateTime.Today.AddDays(7),
                TotalPrice = payment.TotalPrice,
                Deposit = payment.Deposit,
                DepositDueDate = payment.DepositDueDate,
                Remaining = payment.Remaining,
                RemainingDueDate = payment.RemainingDueDate,
                CreatedAt = DateTime.Now,

                // Dodatkowe dla widoku maila
                DepositPercentage = settings.DepositPercentage,
                Currency = settings.Currency ?? "PLN"
            };

            return View("~/Views/Email/NewReservationClient.cshtml", model);
        }

        // Podgląd dla właściciela
        public async Task<IActionResult> PreviewOwner()
        {
            var house = await _context.Houses
                .Include(h => h.Contacts!)
                    .ThenInclude(c => c.Addresses)
                .Include(h => h.Contacts!)
                    .ThenInclude(c => c.PhoneNumbers)
                .Include(h => h.Contacts!)
                    .ThenInclude(c => c.EmailAddresses)
                .Include(h => h.Settings)
                .FirstOrDefaultAsync(h => h.HouseId == 1);

            if (house == null)
                return NotFound("Nie znaleziono domu o ID 1.");

            //var settings = house.Settings ?? new HouseSettings();
            var settings = house.HouseSettingsId != null ? await _context.HouseSettings.FirstOrDefaultAsync(s => s.Id == house.HouseSettingsId)
                                 : await _context.HouseSettings.FirstOrDefaultAsync(s => s.IsDefault);


            var payment = new PaymentCalculator(
                totalPrice: 1200m,
                arrivalDate: DateTime.Today,
                settings: settings
            );

            var model = new NewReservationEmailViewModel
            {
                HouseName = house.Name,
                HouseLogoUrl = settings.LogoFileName,
                RentalRules = house.RentalRules,
                Contacts = house.Contacts.Select(c => new ContactEmailModel
                {
                    Name = c.Name,
                    Phones = c.PhoneNumbers?.Select(p => p.Number).ToList() ?? new List<string>(),
                    Emails = c.EmailAddresses?.Select(e => e.Email).ToList() ?? new List<string>(),
                    Addresses = c.Addresses?.Select(a => $"{a.Street}, {a.PostalCode} {a.City}, {a.Country}").ToList() ?? new List<string>()
                }).ToList(),

                ClientFullName = "Jan Kowalski",
                ClientEmail = "jan@test.pl",
                ClientPhone = "123 456 789",
                ClientStreet = "Przykładowa",
                ClientNumber = "1",
                ClientZIPCode = "00-000",
                ClientCity = "Warszawa",
                ClientCountry = "Polska",

                From = DateTime.Today,
                To = DateTime.Today.AddDays(7),
                TotalPrice = payment.TotalPrice,
                Deposit = payment.Deposit,
                DepositDueDate = payment.DepositDueDate,
                Remaining = payment.Remaining,
                RemainingDueDate = payment.RemainingDueDate,
                CreatedAt = DateTime.Now,

                DepositPercentage = settings.DepositPercentage,
                Currency = settings.Currency ?? "PLN"
            };

            return View("~/Views/Email/NewReservationOwner.cshtml", model);
        }
    }

    // ---------------- PaymentCalculator ----------------
    public class PaymentCalculator
    {
        public decimal TotalPrice { get; }
        public decimal Deposit { get; }
        public decimal Remaining { get; }
        public DateTime DepositDueDate { get; }
        public DateTime RemainingDueDate { get; }

        public PaymentCalculator(decimal totalPrice, DateTime arrivalDate, HouseSettings settings)
        {
            TotalPrice = totalPrice;

            // Zaliczka
            Deposit = Math.Round(TotalPrice * settings.DepositPercentage / 100m, 2);
            DepositDueDate = DateTime.Now.AddDays(settings.DepositDueDays);

            // Pozostała kwota
            Remaining = TotalPrice - Deposit;
            RemainingDueDate = arrivalDate.AddDays(-settings.FullPaymentDueDaysBeforeArrival);

            // Jeśli termin jest za krótki, pobieramy całość od razu
            if (Remaining <= 0 || arrivalDate <= DateTime.Now.AddDays(settings.FullPaymentDueDaysBeforeArrival))
            {
                Deposit = TotalPrice;
                DepositDueDate = DateTime.Now;
                Remaining = 0;
                RemainingDueDate = DateTime.Now;
            }
        }
    }
}