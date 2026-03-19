using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Services;
using Mail;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace HouseNet9.BackgroundService
{

    //klasa wczesniej dziedziczyła po BackgroundService i uzywała petli while
    public class ReservationPaymentJob : IJob
    {
        private readonly ApplicationDbContext _context;
        private readonly IReservationNotificationService _notificationService;

        public ReservationPaymentJob(ApplicationDbContext context, IReservationNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await CheckReservations();
        }

        private async Task CheckReservations()
        {
            var reservations = await _context.RentalHouses
                .Include(r => r.RentalStatus)
                .Include(r => r.RentalClient)
                .Include(r => r.House)
                    .ThenInclude(h => h.Contacts)
                        .ThenInclude(c => c.EmailAddresses)
                .Include(r => r.House)
                    .ThenInclude(h => h.Settings)
                .Where(r => r.IsActive)
                .ToListAsync();


            var today = DateTime.Today;
            var cancelledStatus = await _context.RentalStatus.FirstOrDefaultAsync(s => s.Name == "Anulowano");

            foreach (var reservation in reservations)
            {
                var statusName = reservation.RentalStatus?.Name;

                // --- 2 dni przed zaliczką ---
                if( statusName == "Do zapłaty" &&
                    reservation.DepositDueDate.HasValue &&
                    !reservation.DepositReminderSent &&
                    today == reservation.DepositDueDate.Value.AddDays(-2).Date)
                {
                    //wysyłąm maila
                    await _notificationService.SendDepositReminderAsync(reservation);
                    reservation.DepositReminderSent = true;
                }

                // --- anulowanie dzień po terminie zaliczki ---
                if (statusName == "Do zapłaty" &&
                    reservation.DepositDueDate.HasValue &&
                    today > reservation.DepositDueDate.Value.Date)
                {
                    if (cancelledStatus != null)
                        reservation.RentalStatusID = cancelledStatus.RentalStatusID;
                    
                    reservation.Annotations += $"{DateTime.Now:yyyy-MM-dd HH:mm} - usunięto automatycznie rezerwację\n";
                    await _notificationService.SendReservationCancelledAsync(reservation);
                }

                // --- 2 dni przed pełną płatnością ---
                if (statusName == "Zaliczka" &&
                    reservation.RemainingDueDate.HasValue &&
                    !reservation.RemainingReminderSent &&
                    today == reservation.RemainingDueDate.Value.AddDays(-2).Date)
                {
                    await _notificationService.SendRemainingReminderAsync(reservation);
                    reservation.RemainingReminderSent = true;
                }
            }

            await _context.SaveChangesAsync();

        }
    }

}
