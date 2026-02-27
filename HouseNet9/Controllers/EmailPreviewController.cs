using HouseNet9.Data;
using HouseNet9.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            // Pobieramy dom z bazy wraz z kontaktami i adresami
            var house = await _context.Houses
                .Include(h => h.Contacts!)
                    .ThenInclude(c => c.Addresses)
                .Include(h => h.Contacts!)
                    .ThenInclude(c => c.PhoneNumbers)
                .Include(h => h.Contacts!)
                    .ThenInclude(c => c.EmailAddresses)
                .FirstOrDefaultAsync(h => h.HouseId == 1);

            if (house == null)
                return NotFound("Nie znaleziono domu o ID 1.");

            // Tworzymy model email z pełnymi danymi klienta i kontaktami domu
            var model = new NewReservationEmailViewModel
            {
                // ------------------- Dane domu -------------------
                HouseName = house.Name,
                HouseLogoUrl = null,
                RentalRules = house.RentalRules,
                Contacts = house.Contacts.Select(c => new ContactEmailModel
                {
                    Name = c.Name,
                    Phones = c.PhoneNumbers?.Select(p => p.Number).ToList() ?? new List<string>(),
                    Emails = c.EmailAddresses?.Select(e => e.Email).ToList() ?? new List<string>(),
                    Addresses = c.Addresses?.Select(a => $"{a.Street}, {a.PostalCode} {a.City}, {a.Country}").ToList() ?? new List<string>()
                }).ToList(),

                // ------------------- Dane klienta -------------------
                ClientFullName = "Jan Kowalski",
                ClientEmail = "jan@test.pl",
                ClientPhone = "123 456 789",
                ClientStreet = "Przykładowa",
                ClientNumber = "1",
                ClientZIPCode = "00-000",
                ClientCity = "Warszawa",
                ClientCountry = "Polska",

                // ------------------- Dane rezerwacji -------------------
                From = DateTime.Today,
                To = DateTime.Today.AddDays(7),
                TotalPrice = 1200,
                Deposit = 360,
                DepositDueDate = DateTime.Today.AddDays(3),
                Remaining = 840,
                RemainingDueDate = DateTime.Today.AddDays(-7),
                CreatedAt = DateTime.Now
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
                .FirstOrDefaultAsync(h => h.HouseId == 1);

            if (house == null)
                return NotFound("Nie znaleziono domu o ID 1.");

            var model = new NewReservationEmailViewModel
            {
                // ------------------- Dane domu -------------------
                HouseName = house.Name,
                HouseLogoUrl = null,
                RentalRules = house.RentalRules,
                Contacts = house.Contacts.Select(c => new ContactEmailModel
                {
                    Name = c.Name,
                    Phones = c.PhoneNumbers?.Select(p => p.Number).ToList() ?? new List<string>(),
                    Emails = c.EmailAddresses?.Select(e => e.Email).ToList() ?? new List<string>(),
                    Addresses = c.Addresses?.Select(a => $"{a.Street}, {a.PostalCode} {a.City}, {a.Country}").ToList() ?? new List<string>()
                }).ToList(),

                // ------------------- Dane klienta -------------------
                ClientFullName = "Jan Kowalski",
                ClientEmail = "jan@test.pl",
                ClientPhone = "123 456 789",
                ClientStreet = "Przykładowa",
                ClientNumber = "1",
                ClientZIPCode = "00-000",
                ClientCity = "Warszawa",
                ClientCountry = "Polska",

                // ------------------- Dane rezerwacji -------------------
                From = DateTime.Today,
                To = DateTime.Today.AddDays(7),
                TotalPrice = 1200,
                Deposit = 360,
                DepositDueDate = DateTime.Today.AddDays(3),
                Remaining = 840,
                RemainingDueDate = DateTime.Today.AddDays(-7),
                CreatedAt = DateTime.Now
            };

            return View("~/Views/Email/NewReservationOwner.cshtml", model);
        }
    }
}
