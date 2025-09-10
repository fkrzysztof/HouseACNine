using HouseNet9.Data;
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

        public IViewComponentResult Invoke()
        {
            var contacts = _context.Contacts
                .Include(c => c.Addresses)
                .Include(c => c.EmailAddresses)
                .Include(c => c.PhoneNumbers)
                .ToList();

            return View(contacts);
        }
    }
}
