using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HouseNet9.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {

            var houseWithGenInfo = await _context.Houses
            .Include(i => i.GeneralInformation)
                .ThenInclude(i => i.Image)
            .Include(i => i.DescriptionPages)
                .ThenInclude(i => i.Image)
            .Include(i => i.DetailedInformation)
                .ThenInclude(i => i.Image)
            .Include(i => i.DetailedInformation)
                .ThenInclude(i => i.DetailedInformationItems)
            .Include(i => i.Distances)
                .ThenInclude(i => i.Image)
            .Include(i => i.Distances)
                .ThenInclude(i => i.DistanceItems)
            .FirstOrDefaultAsync();

            if (houseWithGenInfo?.GeneralInformation == null)
            {
                return NotFound();
            }


            //ViewBag.DescriptionPages = await _context.Houses.Where(f => f.HouseId == houseWithGenInfo.HouseId)
            //    .Include(i => i.DescriptionPages)
            //    .ThenInclude(i => i.Image)
            //    .ToListAsync();
            //ViewBag.GeneralInformation = await _context.Houses.Where(f => f.HouseId == houseWithGenInfo.HouseId)
            //    .Include(i => i.GeneralInformation)
            //    .ThenInclude(i => i.Image)
            //    .ToListAsync();

            //ViewBag.DetailedInformation = await _context.Houses.Where(f => f.HouseId == houseWithGenInfo.HouseId)
            //    .Include(i => i.DetailedInformation)
            //        .ThenInclude(i => i.Image)
            //    .Include(i => i.DetailedInformation)
            //        .ThenInclude(i => i.DetailedInformationItems)
            //    .ToListAsync();

            //ViewBag.Distances = await _context.Houses.Where(f => f.HouseId == houseWithGenInfo.HouseId)
            //    .Include(i => i.Distances)
            //        .ThenInclude(i => i.Image)
            //    .ToListAsync();

            //HttpContext.Session.SetInt32("CurrentHouseId", houseWithGenInfo.HouseId);


            return View(houseWithGenInfo);
           // return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
