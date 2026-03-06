using Data.Data;
using Data.Data.HouseRentalData;
using HouseNet9.Data;
using Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace HouseNet9.BackgroundJobs
{
    public class ReservationPaymentService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailService _emailService;
        private readonly IRazorRenderer _razorRenderer;

        public ReservationPaymentService(IServiceScopeFactory scopeFactory,
                                         IEmailService emailService,
                                         IRazorRenderer razorRenderer)
        {
            _scopeFactory = scopeFactory;
            _emailService = emailService;
            _razorRenderer = razorRenderer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckReservations();

                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

        private async Task CheckReservations()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var reservations = await context.RentalHouses
                .Include(r => r.RentalStatus)
                .Include(r => r.RentalClient)
                .Include(r => r.House)
                    .ThenInclude(h => h.Contacts)
                        .ThenInclude(c => c.EmailAddresses)
                .Where(r => r.IsActive)
                .ToListAsync();

            var today = DateTime.Today;

            foreach (var reservation in reservations)
            {
                // --- 2 dni przed zaliczką ---
                if (reservation.RentalStatus.Name == "Do zapłaty" &&
                    reservation.DepositDueDate.HasValue &&
                    today == reservation.DepositDueDate.Value.AddDays(-2).Date)
                {
                    await SendReminderEmail(reservation, "Zaliczka wkrótce do zapłaty", "Email/ReminderDeposit");
                }

                // --- anulowanie dzień po terminie zaliczki ---
                if (reservation.RentalStatus.Name == "Do zapłaty" &&
                    reservation.DepositDueDate.HasValue &&
                    today > reservation.DepositDueDate.Value.Date)
                {
                    var cancelledStatus = await context.RentalStatus.FirstOrDefaultAsync(s => s.Name == "Cancelled");
                    if (cancelledStatus != null)
                        reservation.RentalStatusID = cancelledStatus.RentalStatusID;

                    await context.SaveChangesAsync();

                    await SendReminderEmail(reservation, "Rezerwacja anulowana", "Email/ReservationCancelled");
                }

                // --- 2 dni przed pełną płatnością ---
                if (reservation.RentalStatus.Name == "Zaliczka" &&
                    reservation.RemainingDueDate.HasValue &&
                    today == reservation.RemainingDueDate.Value.AddDays(-2).Date)
                {
                    await SendReminderEmail(reservation, "Pozostała kwota wkrótce do zapłaty", "Email/ReminderRemaining");
                }
            }

            await context.SaveChangesAsync();
        }

        private async Task SendReminderEmail(RentalHouse reservation, string subject, string viewName)
        {
            var client = reservation.RentalClient;
            if (client == null) return;

            var model = new
            {
                ClientName = client.FullName,
                From = reservation.From,
                To = reservation.To,
                Deposit = reservation.DepositAmount,
                DepositDueDate = reservation.DepositDueDate,
                Remaining = reservation.RemainingAmount,
                RemainingDueDate = reservation.RemainingDueDate,
                TotalPrice = reservation.ToPay,
                HouseName = reservation.House?.Name,
                Currency = reservation.House?.HouseSettings?.Currency ?? "PLN"
            };

            // --- wysyłka do klienta ---
            var body = await _razorRenderer.RenderViewToStringAsync(viewName, model);
            await _emailService.SendEmailAsync(client.Email, subject, body);

            // --- wysyłka do adminów/właścicieli ---
            var adminEmails = reservation.House?.Contacts?
                .SelectMany(c => c.EmailAddresses)
                .Select(e => e.Email)
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct();

            if (adminEmails != null)
            {
                foreach (var email in adminEmails)
                {
                    await _emailService.SendEmailAsync(email, subject, body);
                }
            }
        }
    }
}