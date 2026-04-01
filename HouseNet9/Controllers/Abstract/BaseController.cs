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

        public BaseController(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            int? id = null;

            if (context.RouteData.Values["id"] != null)
                id = Convert.ToInt32(context.RouteData.Values["id"]);

            var query = _context.Houses
                .Where(h => h.IsActive)
                .AsNoTracking();

            if (id.HasValue)
                query = query.Where(h => h.HouseId == id.Value);
            else
                query = query.OrderBy(h => h.HouseId);

            var house = await query.FirstOrDefaultAsync();

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
                ViewBag.IdHouse = id;
            }

            await next();
        }
    }
}