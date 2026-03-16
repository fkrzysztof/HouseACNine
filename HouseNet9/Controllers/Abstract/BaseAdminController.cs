namespace HouseNet9.Controllers.Abstract
{
    using global::HouseNet9.Data;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    namespace HouseNet9.Controllers.Admin
    {
        //[Authorize]
        public class BaseAdminController : Controller
        {
            protected readonly ApplicationDbContext _context;
            protected readonly ILogger _logger;

            protected int? CurrentHouseId => HttpContext.Session.GetInt32("AdminCurrentHouseId");
            protected string? CurrentHouseName => HttpContext.Session.GetString("AdminCurrentHouseName");

            public BaseAdminController(ApplicationDbContext context, ILoggerFactory loggerFactory)
            {
                _context = context;
                _logger = loggerFactory.CreateLogger(GetType());
            }

            public override async Task OnActionExecutionAsync(
                ActionExecutingContext context,
                ActionExecutionDelegate next)
            {
                // jeśli nie wybrano domu → wróć do listy domów
                if (CurrentHouseId == null &&
                    context.Controller.GetType().Name != "HousesController")
                {
                    context.Result = RedirectToAction("Index", "Houses");
                    return;
                }

                //ViewBag.AdminCurrentHouseName = CurrentHouseName;

                await next();
            }
        }
    }
}
