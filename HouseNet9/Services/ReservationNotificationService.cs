using Data.Data.HouseRentalData;
using HouseNet9.ViewModels;
using Mail;

namespace HouseNet9.Services
{
    public class ReservationNotificationService : IReservationNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IRazorViewToStringRenderer _razorRenderer;

        public ReservationNotificationService(IEmailService emailService, IRazorViewToStringRenderer razorRenderer)
        {
            _emailService = emailService;
            _razorRenderer = razorRenderer;
        }

        private NewReservationEmailViewModel BuildEmailModel(RentalHouse reservation, string messageTitle, string customMessage)
        {
            var settings = reservation.House?.Settings;

            return new NewReservationEmailViewModel
            {
                HouseName = reservation.House?.Name ?? "",
                HouseLogoUrl = settings?.LogoFileName,
                From = reservation.From,
                To = reservation.To,
                TotalPrice = reservation.ToPay,
                Deposit = reservation.DepositAmount,
                DepositDueDate = reservation.DepositDueDate ?? DateTime.Today,
                Remaining = reservation.RemainingAmount,
                RemainingDueDate = reservation.RemainingDueDate ?? DateTime.Today,
                ClientFullName = reservation.RentalClient?.FullName ?? "",
                ClientEmail = reservation.RentalClient?.Email ?? "",
                ClientPhone = reservation.RentalClient?.Phone ?? "",
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
                MessageTitle = messageTitle,
                CustomMessage = customMessage,
                BankName = settings?.BankName,
                BankAccountIban = settings?.BankAccountIban,
                BankAccountSwift = settings?.BankAccountSwift,
                BankAccountOwner = settings?.BankAccountName,
                PaymentReference = $"Rezerwacja {reservation.ReservationNumber}"
            };
        }

        private async Task SendEmailAsync(RentalHouse reservation, NewReservationEmailViewModel model, string viewName)
        {
            // Do klienta
            if (!string.IsNullOrEmpty(reservation.RentalClient?.Email))
            {
                var body = await _razorRenderer.RenderViewToStringAsync(viewName, model);
                await _emailService.SendEmailAsync(reservation.RentalClient.Email, model.MessageTitle ?? "", body);
            }

            // Do kontaktów w domu
            var adminEmails = reservation.House?.Contacts?
                .SelectMany(c => c.EmailAddresses)
                .Select(e => e.Email)
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct();

            if (adminEmails != null)
            {
                var body = await _razorRenderer.RenderViewToStringAsync(viewName, model);
                foreach (var email in adminEmails)
                {
                    await _emailService.SendEmailAsync(email, model.MessageTitle ?? "", body);
                }
            }
        }

        public Task SendDepositReminderAsync(RentalHouse reservation)
        {
            var model = BuildEmailModel(reservation, "Przypomnienie o zaliczce", $"Prosimy o wpłatę zaliczki ({reservation.DepositAmount} {reservation.House?.Settings?.Currency}) przed {reservation.DepositDueDate?.ToShortDateString()}");
            return SendEmailAsync(reservation, model, "Email/ReminderDeposit");
        }

        public Task SendRemainingReminderAsync(RentalHouse reservation)
        {
            var model = BuildEmailModel(reservation, "Przypomnienie o pozostałej kwocie", $"Prosimy o wpłatę pozostałej kwoty ({reservation.RemainingAmount} {reservation.House?.Settings?.Currency}) przed {reservation.RemainingDueDate?.ToShortDateString()}");
            return SendEmailAsync(reservation, model, "Email/ReminderRemaining");
        }

        public Task SendDepositConfirmedAsync(RentalHouse reservation)
        {
            var model = BuildEmailModel(reservation, "Potwierdzenie otrzymania zaliczki", $"Otrzymaliśmy zaliczkę ({reservation.DepositAmount} {reservation.House?.Settings?.Currency}). Dziękujemy!");
            return SendEmailAsync(reservation, model, "Email/DepositConfirmed");
        }

        public Task SendFullPaymentConfirmedAsync(RentalHouse reservation)
        {
            var model = BuildEmailModel(reservation, "Potwierdzenie pełnej płatności", $"Otrzymaliśmy pełną płatność ({reservation.ToPay} {reservation.House?.Settings?.Currency}). Dziękujemy!");
            return SendEmailAsync(reservation, model, "Email/FullPaymentConfirmed");
        }

        public Task SendReservationCancelledAsync(RentalHouse reservation)
        {
            var model = BuildEmailModel(reservation, "Rezerwacja anulowana", $"Twoja rezerwacja ({reservation.From.ToShortDateString()} – {reservation.To.ToShortDateString()}) została anulowana.");
            return SendEmailAsync(reservation, model, "Email/ReservationCancelled");
        }
    }
}
