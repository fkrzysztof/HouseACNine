using Data.Data.HouseRentalData;

namespace HouseNet9.Services.Payments
{
    public interface IPaymentCalculator
    {
        PaymentCalculationResult Calculate(
            decimal totalPrice,
            DateTime arrivalDate,
            HouseSettings settings);
    }
}
