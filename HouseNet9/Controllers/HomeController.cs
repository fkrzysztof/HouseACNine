using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Models;
using Microsoft.AspNetCore.Authorization;
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



        [Authorize]
        public IActionResult Secret()
        {
            return Content("Zalogowany 👍");
        }

        //public async Task<IActionResult> Index(int id)
        public async Task<IActionResult> Index()
        {
            House? houseWithGenInfo = await _context.Houses
              //  .Where(w => w.HouseId == id)
            .Include(i => i.GeneralInformation)
                .ThenInclude(i => i.Image)
            .Include(i => i.DescriptionPages)
                .ThenInclude(i => i.Images).Take(1)
            .Include(i => i.Distances)
                .ThenInclude(i => i.Image)
            .Include(i => i.Distances)
                .ThenInclude(i => i.DistanceItems)
            .FirstOrDefaultAsync(); //do zmiany id - get!


            if (houseWithGenInfo?.GeneralInformation == null)
            {
                return NotFound();
            }

            //HttpContext.Session.SetInt32("CurrentHouseId", houseWithGenInfo.HouseId);
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


        public async Task<IActionResult> More(int id, int houseId)
        {

            DescriptionPage? dp = await _context.DescriptionPages.Where(w => w.DescriptionPageId == id)
                .Include(i => i.Images)
                .Include(i => i.House)
                .FirstOrDefaultAsync();
            
            GeneralInformation? gi = await _context.GeneralInformation.Where(w => w.House.HouseId == houseId)
                .Include(i => i.Image)
                .FirstOrDefaultAsync();

            List<DetailedInformation> di = await _context.DetailedInformation.Where(w => w.House.HouseId == houseId)
                .Include(i => i.Image)
                .Include(i => i.DetailedInformationItems)
                .ToListAsync();

            ViewBag.Detailed = di;
            
            return View(dp);
        }

        }
}
