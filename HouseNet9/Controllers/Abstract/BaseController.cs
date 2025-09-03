using HouseNet9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers.Abstract
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // Pobierz aktualny HouseId z Session
            var houseId = HttpContext.Session.GetInt32("CurrentHouseId");

            //na chwile
            houseId = 1;

            if (houseId.HasValue)
            {
                // Pobierz kontakty dla tego domu
                var contacts = _context.Contacts
                    .Include(c => c.PhoneNumbers)
                    .Include(c => c.EmailAddresses)
                    .Include(c => c.Addresses)
                    .Where(c => c.HouseId == houseId.Value)
                    .ToList();

                ViewData["Contacts"] = contacts;
            }
        }
    }
}
