//using HouseNet9.Data;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.EntityFrameworkCore;

//namespace HouseNet9.Controllers.Abstract
//{
//    public class BaseController : Controller
//    {
//        protected readonly ApplicationDbContext _context;
//        protected readonly ILogger _logger;

//        public BaseController(ApplicationDbContext context, ILoggerFactory loggerFactory)
//        {
//            _context = context;
//            _logger = loggerFactory.CreateLogger(GetType());
//        }

//        public override async Task OnActionExecutionAsync(
//            ActionExecutingContext context,
//            ActionExecutionDelegate next)
//        {
//            int? id = null;

//            if (context.RouteData.Values["id"] != null)
//                id = Convert.ToInt32(context.RouteData.Values["id"]);

//            var query = _context.Houses
//                .Where(h => h.IsActive)
//                .AsNoTracking();

//            if (id.HasValue)
//                query = query.Where(h => h.HouseId == id.Value);
//            else
//                query = query.OrderBy(h => h.HouseId);

//            var house = await query.FirstOrDefaultAsync();

//            if (house != null)
//            {
//                var settings = house.HouseSettingsId != null
//                    ? await _context.HouseSettings
//                        .FirstOrDefaultAsync(s => s.Id == house.HouseSettingsId)
//                    : await _context.HouseSettings
//                        .FirstOrDefaultAsync(s => s.IsDefault);

//                ViewBag.HouseName = house.Name;
//                ViewBag.ShortText = house.ShortText;
//                ViewBag.LongText = house.LongText;
//                ViewBag.Logo = settings?.LogoFileName;
//                ViewBag.IdHouse = house.HouseId;

//                var dp = await _context.DescriptionPages.Where(w => w.HouseId == house.HouseId).Select(s => s.Title).ToListAsync();

//                if (dp != null && dp.Any() == true)
//                {
//                    ViewBag.MenuItems = dp;
//                }
//                else
//                {
//                    ViewBag.MenuItems = null;
//                }

//                await next();
//            }
//        }
//    }
//}


using Data.Data.HouseRentalData;
using HouseNet9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers.Abstract
{
    public class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger _logger;

        public BaseController(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            // ---------------------------------------------
            // AKTUALNY CONTROLLER / ACTION
            // ---------------------------------------------
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            int? houseId = null;

            // ---------------------------------------------
            // TYLKO DLA WYBRANYCH AKCJI POBIERAMY HOUSE ID
            // ---------------------------------------------

            // Home/Index/5
            if (controllerName == "Home")
            {
                if (context.RouteData.Values["id"] != null)
                {
                    houseId = Convert.ToInt32(context.RouteData.Values["id"]);
                }
            }

            // House/Details/5
            if (controllerName == "House")
            {
                if (context.RouteData.Values["id"] != null)
                {
                    houseId = Convert.ToInt32(context.RouteData.Values["id"]);
                }
            }

            // ---------------------------------------------
            // JEŚLI NIE MA ID -> POBIERZ PIERWSZY AKTYWNY DOM
            // ---------------------------------------------
            IQueryable<House> query = _context.Houses
                .Where(h => h.IsActive)
                .AsNoTracking();

            if (houseId.HasValue)
            {
                query = query.Where(h => h.HouseId == houseId.Value);
            }
            else
            {
                query = query.OrderBy(h => h.HouseId);
            }

            var house = await query.FirstOrDefaultAsync();

            // ---------------------------------------------
            // JEŚLI DOM ISTNIEJE -> UZUPEŁNIJ VIEWBAG
            // ---------------------------------------------
            if (house != null)
            {
                var settings = house.HouseSettingsId != null
                    ? await _context.HouseSettings
                        .FirstOrDefaultAsync(s => s.Id == house.HouseSettingsId)
                    : await _context.HouseSettings
                        .FirstOrDefaultAsync(s => s.IsDefault);

                ViewBag.HouseName = house.Name;
                ViewBag.ShortText = house.ShortText;
                ViewBag.LongText = house.LongText;
                ViewBag.Logo = settings?.LogoFileName;
                ViewBag.IdHouse = house.HouseId;

                var descriptionPages = await _context.DescriptionPages
                    .Where(w => w.HouseId == house.HouseId)
                    .Select(s => s.Title)
                    .ToListAsync();

                ViewBag.MenuItems = descriptionPages.Any()
                    ? descriptionPages
                    : null;
            }

            // ---------------------------------------------
            // NAJWAŻNIEJSZE
            // MVC MUSI IŚĆ DALEJ
            // ---------------------------------------------
            await next();
        }
    }
}