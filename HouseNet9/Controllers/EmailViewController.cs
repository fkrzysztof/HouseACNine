using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Services;
using HouseNet9.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers
{
    [Route("email/preview")]
    public class EmailViewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmailViewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lista rezerwacji do podglądu
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var reservations = await _context.RentalHouses
                .Include(r => r.RentalClient)
                .Include(r => r.House)
                    .ThenInclude(h => h.Settings)
                .Take(20)
                .ToListAsync();

            return View(reservations); // Widok: Email/List.cshtml
        }

        // --- Podgląd zaliczki ---
        [HttpGet("deposit/{reservationNumber}")]
        public async Task<IActionResult> DepositReminder(string reservationNumber)
        {
            var reservation = await LoadReservation(reservationNumber);
            if (reservation == null) return NotFound();

            var model = BuildEmailModel(reservation,
                "Przypomnienie o zaliczce",
                $"Prosimy o wpłatę zaliczki ({reservation.DepositAmount} {reservation.House?.Settings?.Currency}) przed {reservation.DepositDueDate?.ToShortDateString()}");

            return View("~/Views/Email/ReminderDeposit.cshtml", model);
        }

        // --- Podgląd pozostałej kwoty ---
        [HttpGet("remaining/{reservationNumber}")]
        public async Task<IActionResult> RemainingReminder(string reservationNumber)
        {
            var reservation = await LoadReservation(reservationNumber);
            if (reservation == null) return NotFound();

            var model = BuildEmailModel(reservation,
                "Przypomnienie o pozostałej kwocie",
                $"Prosimy o wpłatę pozostałej kwoty ({reservation.RemainingAmount} {reservation.House?.Settings?.Currency}) przed {reservation.RemainingDueDate?.ToShortDateString()}");

            return View("~/Views/Email/ReminderRemaining.cshtml", model);
        }

        // --- Podgląd potwierdzenia zaliczki ---
        [HttpGet("depositconfirmed/{reservationNumber}")]
        public async Task<IActionResult> DepositConfirmed(string reservationNumber)
        {
            var reservation = await LoadReservation(reservationNumber);
            if (reservation == null) return NotFound();

            var model = BuildEmailModel(reservation,
                "Potwierdzenie otrzymania zaliczki",
                $"Otrzymaliśmy zaliczkę ({reservation.DepositAmount} {reservation.House?.Settings?.Currency}). Dziękujemy!");

            return View("~/Views/Email/DepositConfirmed.cshtml", model);
        }

        // --- Podgląd potwierdzenia pełnej płatności ---
        [HttpGet("fullpaymentconfirmed/{reservationNumber}")]
        public async Task<IActionResult> FullPaymentConfirmed(string reservationNumber)
        {
            var reservation = await LoadReservation(reservationNumber);
            if (reservation == null) return NotFound();

            var model = BuildEmailModel(reservation,
                "Potwierdzenie pełnej płatności",
                $"Otrzymaliśmy pełną płatność ({reservation.ToPay} {reservation.House?.Settings?.Currency}). Dziękujemy!");

            return View("~/Views/Email/FullPaymentConfirmed.cshtml", model);
        }

        // --- Podgląd anulowania ---
        [HttpGet("cancelled/{reservationNumber}")]
        public async Task<IActionResult> ReservationCancelled(string reservationNumber)
        {
            var reservation = await LoadReservation(reservationNumber);
            if (reservation == null) return NotFound();

            var model = BuildEmailModel(reservation,
                "Rezerwacja anulowana",
                $"Twoja rezerwacja ({reservation.From.ToShortDateString()} – {reservation.To.ToShortDateString()}) została anulowana.");

            return View("~/Views/Email/ReservationCancelled.cshtml", model);
        }

        // Pomocnicza metoda ładowania rezerwacji po numerze
        private async Task<RentalHouse?> LoadReservation(string reservationNumber)
        {
            return await _context.RentalHouses
                .Include(r => r.RentalClient)
                .Include(r => r.House)
                    .ThenInclude(h => h.Settings)
                .Include(r => r.House)
                    .ThenInclude(h => h.Contacts)
                        .ThenInclude(c => c.EmailAddresses)
                .Include(r => r.House)
                    .ThenInclude(h => h.Contacts)
                        .ThenInclude(c => c.PhoneNumbers)
                .Include(r => r.House)
                    .ThenInclude(h => h.Contacts)
                        .ThenInclude(c => c.Addresses)
                .FirstOrDefaultAsync(r => r.ReservationNumber == reservationNumber);
        }

        // Pomocnicza metoda budowania ViewModel
        private NewReservationEmailViewModel BuildEmailModel(RentalHouse reservation, string title, string message)
        {
            var settings = reservation.House?.Settings
               ?? _context.HouseSettings.FirstOrDefault(s => s.IsDefault);

            return new NewReservationEmailViewModel
            {
                HouseName = reservation.House?.Name ?? "",
                HouseLogoUrl = settings?.LogoFileName,

                ReservationNumber = reservation.ReservationNumber ?? "TEST123",

                From = reservation.From,
                To = reservation.To,

                TotalPrice = reservation.ToPay,
                Deposit = reservation.DepositAmount,
                DepositDueDate = reservation.DepositDueDate ?? DateTime.Today,

                Remaining = reservation.RemainingAmount,
                RemainingDueDate = reservation.RemainingDueDate ?? DateTime.Today,

                ClientFullName = reservation.RentalClient?.FullName ?? "Jan Kowalski",
                ClientEmail = reservation.RentalClient?.Email ?? "test@example.com",
                ClientPhone = reservation.RentalClient?.Phone ?? "123456789",

                ClientStreet = reservation.RentalClient?.Street ?? "",
                ClientNumber = reservation.RentalClient?.Number ?? "",
                ClientZIPCode = reservation.RentalClient?.ZIPCode ?? "",
                ClientCity = reservation.RentalClient?.City ?? "",
                ClientCountry = reservation.RentalClient?.Country ?? "",

                CreatedAt = reservation.CreationDate,

                Contacts = reservation.House?.Contacts?.Select(c => new ContactEmailModel
                {
                    Name = c.Name,
                    Emails = c.EmailAddresses?.Select(e => e.Email).ToList(),
                    Phones = c.PhoneNumbers?.Select(p => p.Number).ToList(),
                    Addresses = c.Addresses?.Select(a => a.FullAddress).ToList()
                }).ToList(),

                Currency = settings?.Currency ?? "€",
                DepositPercentage = settings?.DepositPercentage ?? 30,

                MessageTitle = title,
                CustomMessage = message,

                BankName = settings?.BankName,
                BankAccountIban = settings?.BankAccountIban,
                BankAccountSwift = settings?.BankAccountSwift,
                BankAccountOwner = settings?.BankAccountName,

                PaymentReference = $"Rezerwacja {reservation.ReservationNumber ?? "TEST123"}"
            };
        }
    }
}