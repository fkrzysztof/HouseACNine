using Data.Data.HouseRentalData;
using WeatherModule.Core.Models;

namespace HouseNet9.ViewModels
{
    public class ContactFooterVM
    {
        public List<Contact> Contacts { get; set; }
        public ContactFormModel Form { get; set; } = new ContactFormModel();


        public WeatherDto Weather { get; set; }
    }
}
