namespace HouseNet9.ViewModels
{
    public class NewReservationOwnerEmailViewModel
    {
        public string HouseName { get; set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public decimal TotalPrice { get; set; }

        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
