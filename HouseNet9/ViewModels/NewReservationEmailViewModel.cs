namespace HouseNet9.ViewModels
{
    public class NewReservationEmailViewModel
    {
        public string HouseName { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public decimal TotalPrice { get; set; }
        public decimal Deposit { get; set; }
        public DateTime DepositDueDate { get; set; }

        public decimal Remaining { get; set; }
        public DateTime RemainingDueDate { get; set; }

        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
