namespace HouseNet9.ViewModels
{
    public class NewReservationEmailViewModel
    {
        public string HouseName { get; set; } = "";
        public string? HouseLogoUrl { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal Deposit { get; set; }
        public DateTime DepositDueDate { get; set; }
        public decimal Remaining { get; set; }
        public DateTime RemainingDueDate { get; set; }
        public string ClientFullName { get; set; } = "";
        public string ClientEmail { get; set; } = "";
        public string ClientPhone { get; set; } = "";
        public string ClientStreet { get; set; } = "";
        public string ClientNumber { get; set; } = "";
        public string ClientZIPCode { get; set; } = "";
        public string ClientCity { get; set; } = "";
        public string ClientCountry { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string? RentalRules { get; set; }
        public List<ContactEmailModel>? Contacts { get; set; } = new();
        public string Currency { get; set; } = "€";
        public decimal DepositPercentage { get; set; }
    }
}
