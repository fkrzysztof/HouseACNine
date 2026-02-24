using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers
{
    public class RentalHouseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RentalCalculatorService _calculator;

        public RentalHouseController(ApplicationDbContext context)
        {
            _context = context;
            _calculator = new RentalCalculatorService(_context);
        }


        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string? clientSearch)
        {

            int idHouse = HttpContext.Session.GetInt32("CurrentHouseId") ?? 0;

            var query = _context.RentalHouses
                .Where(w => w.HouseId == idHouse)
                .Include(r => r.House)
                .Include(r => r.RentalClient)
                .Include(r => r.RentalStatus)
                .Where(r => r.IsActive == true)
                .AsQueryable();

            ViewBag.Statuses = await _context.RentalStatus
                                .OrderBy(s => s.RentalStatusID)
                                .ToListAsync();


            if (fromDate.HasValue)
                query = query.Where(r => r.From >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.To <= toDate.Value);

            //  Wyszukiwanie po fragmencie nazwiska imienia
            if (!string.IsNullOrWhiteSpace(clientSearch))
            {
                query = query.Where(r =>
                (r.RentalClient.Name + " " + r.RentalClient.LastName)
                .Contains(clientSearch));
            }

            query = query.OrderByDescending(r => r.CreationDate);

            ViewData["ClientSearch"] = clientSearch;
            ViewData["SumPrice"] = query.Sum(p => p.ToPay);


            return View(await query.ToListAsync());
        }




            // GET: Create
            public IActionResult Create()
            {
                var houseId = HttpContext.Session.GetInt32("CurrentHouseId");
                if (houseId == null)
                    return RedirectToAction("Index", "House");

                var model = new RentalHouse
                {
                    HouseId = houseId.Value,
                    From = DateTime.Today,
                    To = DateTime.Today.AddDays(6) // domyślnie 6 dni
                };

                return View(model);
            }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalHouse rentalHouse)
        {
            var houseId = HttpContext.Session.GetInt32("CurrentHouseId");
            if (houseId == null)
                return RedirectToAction("Index", "House");

            rentalHouse.HouseId = houseId.Value;

            // 🔹 Walidacja dat
            if (rentalHouse.To <= rentalHouse.From)
            {
                ModelState.AddModelError("", "Data zakończenia musi być późniejsza niż data rozpoczęcia.");
                return View(rentalHouse);
            }

            // 🔹 Sprawdzenie dostępności terminu
            bool isOccupied = await _context.RentalHouses
                .AnyAsync(r => r.HouseId == rentalHouse.HouseId
                            && r.IsActive
                            && r.From <= rentalHouse.To   // istniejąca rezerwacja zaczyna się przed końcem nowej
                            && r.To >= rentalHouse.From); // istniejąca rezerwacja kończy się po początku nowej


            if (isOccupied)
            {
                ModelState.AddModelError("", "Wybrany termin jest już zajęty.");
                return View(rentalHouse);
            }

            if (ModelState.IsValid)
            {
                rentalHouse.CreationDate = DateTime.Now;
                rentalHouse.IsActive = true;

                // 🔹 Właściciel czy klient?
                if (rentalHouse.RentalClientId == null)
                {
                    rentalHouse.ToPay = 0;
                    rentalHouse.RentalStatusID = await _context.RentalStatus
                        .Where(s => s.Name == "Wynajem własny")
                        .Select(s => s.RentalStatusID)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    rentalHouse.ToPay = await _calculator.CalculatePriceAsync(rentalHouse);
                    rentalHouse.RentalStatusID = await _context.RentalStatus
                        .Where(s => s.Name == "Do zapłaty")
                        .Select(s => s.RentalStatusID)
                        .FirstOrDefaultAsync();
                }

                _context.Add(rentalHouse);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(rentalHouse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatusSelect([FromBody] StatusChangeModel model)
        {
            var rental = await _context.RentalHouses
                .Include(r => r.RentalStatus)
                .FirstOrDefaultAsync(r => r.RentalHouseID == model.Id);

            if (rental == null) return NotFound();

            var status = await _context.RentalStatus
                .FirstOrDefaultAsync(s => s.RentalStatusID == model.StatusId);

            if (status == null) return NotFound();

            rental.RentalStatusID = status.RentalStatusID;
            await _context.SaveChangesAsync();

            // Kolory dla wskaźnika
            string badgeColor = status.Name switch
            {
                "Zapłacono całość" => "green",
                "Do zapłaty" => "yellow",
                "Zaliczka" => "blue",
                "Wynajem własny" => "darkgray",
                _ => "gray"
            };

            return Json(new { statusId = status.RentalStatusID, badgeColor });
        }

        // Model dla AJAX
        public class StatusChangeModel
        {
            public int Id { get; set; }
            public int StatusId { get; set; }
        }







        // POST: RentalHouse/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rentalHouse = await _context.RentalHouses.FindAsync(id);
            if (rentalHouse == null) return NotFound();

            rentalHouse.IsActive = false;
            _context.Update(rentalHouse);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool RentalHouseExists(int id)
        {
            return _context.RentalHouses.Any(e => e.RentalHouseID == id);
        }








    }
}
