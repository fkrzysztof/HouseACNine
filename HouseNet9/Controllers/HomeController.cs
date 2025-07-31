using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HouseNet9.Controllers
{
    public class HomeController : Controller
    {
        //przeniesc do abstract class
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {

            var house = _context.Houses.FirstOrDefault();
            if (house == null) return NotFound();

            var houseWithGenInfo = _context.Houses
                .Include(i => i.GeneralInformation)
                .ThenInclude(i => i.Image)
                .Include(i => i.DescriptionPages)
                .ThenInclude(i => i.Image)
                .Include(i => i.DetailedInformation)
                .ThenInclude(i => i.Image)
                .Include(i => i.DetailedInformation)
                .ThenInclude(i => i.DetailedInformationItems)
                .First();

            if (houseWithGenInfo?.GeneralInformation == null)
            {
                return NotFound();
            }



            return View(houseWithGenInfo);
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
