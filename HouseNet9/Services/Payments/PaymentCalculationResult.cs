namespace HouseNet9.Services.Payments
{
    public class PaymentCalculationResult
    {
        public decimal Deposit { get; set; }
        public DateTime DepositDueDate { get; set; }

        public decimal Remaining { get; set; }
        public DateTime RemainingDueDate { get; set; }

        public decimal Total { get; set; }
    }
}
