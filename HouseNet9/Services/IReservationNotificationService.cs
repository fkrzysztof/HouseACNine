using Data.Data.HouseRentalData;

namespace HouseNet9.Services
{
    public interface IReservationNotificationService
    {
        Task SendDepositReminderAsync(RentalHouse reservation);
        Task SendRemainingReminderAsync(RentalHouse reservation);
        Task SendDepositConfirmedAsync(RentalHouse reservation);
        Task SendFullPaymentConfirmedAsync(RentalHouse reservation);
        Task SendReservationCancelledAsync(RentalHouse reservation);
    }
}
