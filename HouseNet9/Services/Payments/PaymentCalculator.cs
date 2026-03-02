using Data.Data.HouseRentalData;

namespace HouseNet9.Services.Payments
{
    public class PaymentCalculator : IPaymentCalculator
    {
        public PaymentCalculationResult Calculate(
            decimal totalPrice,
            DateTime arrivalDate,
            HouseSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var now = DateTime.Now;

            // Granica pełnej płatności
            var fullPaymentBoundary =
                arrivalDate.AddDays(-settings.FullPaymentDueDaysBeforeArrival);

            decimal deposit;
            decimal remaining;
            DateTime depositDueDate;
            DateTime remainingDueDate;

            // 🔥 LAST MINUTE → 100% od razu
            if (now >= fullPaymentBoundary)
            {
                deposit = totalPrice;
                remaining = 0m;

                depositDueDate = now;
                remainingDueDate = now;
            }
            else
            {
                // Standardowy tryb: zaliczka %
                deposit = Math.Round(
                    totalPrice * settings.DepositPercentage / 100m,
                    2);

                remaining = totalPrice - deposit;

                depositDueDate = now.AddDays(settings.DepositDueDays);

                remainingDueDate = fullPaymentBoundary;

                // Zabezpieczenie: jeśli zaliczka wychodzi w przeszłości
                if (depositDueDate < now)
                    depositDueDate = now;
            }

            return new PaymentCalculationResult
            {
                Total = totalPrice,
                Deposit = deposit,
                DepositDueDate = depositDueDate,
                Remaining = remaining,
                RemainingDueDate = remainingDueDate
            };
        }
    }
}
