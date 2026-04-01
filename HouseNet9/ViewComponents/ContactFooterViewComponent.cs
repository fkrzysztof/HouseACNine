using HouseNet9.Data;
using HouseNet9.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.ViewComponents
{
    public class ContactFooterViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ContactFooterViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }


        public IViewComponentResult Invoke(int? id)
        {
            var contacts = _context.Contacts
                .Where(w => w.HouseId == id)
                .Include(c => c.Addresses)
                .Include(c => c.EmailAddresses)
                .Include(c => c.PhoneNumbers)
                .ToList();

            var vm = new ContactFooterVM
            {
                Contacts = contacts,
                Form = new ContactFormModel()
            };

            return View(vm);
        }



    }
}
