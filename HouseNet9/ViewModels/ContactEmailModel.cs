namespace HouseNet9.ViewModels
{
    public class ContactEmailModel
    {
        public string Name { get; set; }
        public List<string> Phones { get; set; } = new();
        public List<string> Emails { get; set; } = new();
        public List<string> Addresses { get; set; } = new();
    }
}
