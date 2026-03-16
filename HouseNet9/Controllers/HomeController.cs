using Data.Data.HouseRentalData;
using HouseNet9.Controllers.Abstract;
using HouseNet9.Data;
using HouseNet9.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HouseNet9.Controllers
{
    public class HomeController : BaseController
    {


        public HomeController(ILoggerFactory loggerFactory, ApplicationDbContext context)
        : base(context, loggerFactory)
        {
        }



        [Authorize]
        public IActionResult Secret()
        {
            return Content("Zalogowany 👍");
        }

        public async Task<IActionResult> Index(int? id)
        {
            IQueryable<House> query = _context.Houses
                .Where(i => i.IsActive)
                .Include(h => h.GeneralInformation)
                    .ThenInclude(g => g.Image)
                .Include(h => h.DescriptionPages)
                    .ThenInclude(d => d.Images)
                .Include(h => h.Distances)
                    .ThenInclude(d => d.Image)
                .Include(h => h.Distances)
                    .ThenInclude(d => d.DistanceItems)
                .AsNoTracking();

            if (id.HasValue)
                query = query.Where(h => h.HouseId == id.Value);
            else
                query = query.OrderBy(h => h.HouseId);

            var house = await query.FirstOrDefaultAsync();

            if (house == null)
                return NotFound();

            foreach (var descPage in house.DescriptionPages)
            {
                descPage.Images = descPage.Images
                    .OrderBy(img => img.Order)
                    .ToList();
            }

            return View(house);
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
