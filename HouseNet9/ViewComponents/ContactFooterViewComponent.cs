using HouseNet9.Data;
using HouseNet9.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeatherModule.Core.Services;

namespace HouseNet9.ViewComponents
{
    public class ContactFooterViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IWeatherService _weather;

        public ContactFooterViewComponent(ApplicationDbContext context, IWeatherService weather)
        {
            _context = context;
            _weather = weather;
        }


        //public async Task<IViewComponentResult> Invoke(int? id, double lat, double lon)
        //{
        //    var contacts = _context.Contacts
        //        .Where(w => w.HouseId == id)
        //        .Include(c => c.Addresses)
        //        .Include(c => c.EmailAddresses)
        //        .Include(c => c.PhoneNumbers)
        //        .ToList();

        //    var vm = new ContactFooterVM
        //    {
        //        Contacts = contacts,
        //        Form = new ContactFormModel()
        //    };

        //    ViewBag.Weather = await _weather.GetCurrentAsync(lat, lon);

        //    return View(vm);
        //}

        public async Task<IViewComponentResult> InvokeAsync(int? id, double lat, double lon)
        {
            var contacts = await _context.Contacts
                .Where(w => w.HouseId == id)
                .Include(c => c.Addresses)
                .Include(c => c.EmailAddresses)
                .Include(c => c.PhoneNumbers)
                .ToListAsync();

            var weather = await _weather.GetCurrentAsync(lat, lon);

            var vm = new ContactFooterVM
            {
                Contacts = contacts,
                Form = new ContactFormModel(),
                Weather = weather
            };

            return View(vm);
        }



    }
}
