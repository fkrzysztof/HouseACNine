using Data.Data.HouseRentalData;

namespace HouseNet9.ViewModels
{
    public class ContactFooterVM
    {
        public List<Contact> Contacts { get; set; }

        public ContactFormModel Form { get; set; } = new ContactFormModel();
    }
}
