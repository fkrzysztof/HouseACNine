using Data.Data.HouseRentalData;
using HouseNet9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers
{
    public class RentalPricesController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public RentalPricesController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }




        // ===========================
        // INDEX – ceny dla konkretnego domu
        // ===========================
        public async Task<IActionResult> Index(int houseId)
        {
            var house = await _context.Houses.FindAsync(houseId);
            if (house == null) return NotFound();

            ViewBag.House = house;

            var list = await _context.RentalPrices
                .Where(x => x.HouseId == houseId)
                .OrderByDescending(x => x.DateTimeFrom)
                .ToListAsync();

            ViewBag.List = list;

            // model do formularza CREATE
            return View(new RentalPrice { HouseId = houseId });
        }

        // ===========================
        // CREATE
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalPrice rentalPrice)
        {
            rentalPrice.IsActive = true;

            if (ModelState.IsValid)
            {
                _context.RentalPrices.Add(rentalPrice);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { houseId = rentalPrice.HouseId });
            }

            return RedirectToAction(nameof(Index), new { houseId = rentalPrice.HouseId });
        }

        // ===========================
        // EDIT GET
        // ===========================
        public async Task<IActionResult> Edit(int id)
        {
            var rentalPrice = await _context.RentalPrices.FindAsync(id);
            if (rentalPrice == null) return NotFound();

            return View(rentalPrice);
        }

        // ===========================
        // EDIT POST
        // ===========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RentalPrice rentalPrice)
        {
            if (id != rentalPrice.RentalPriceID)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(rentalPrice);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index), new { houseId = rentalPrice.HouseId });
            }

            return View(rentalPrice);
        }

        // ===========================
        // DELETE
        // ===========================
        public async Task<IActionResult> Delete(int id)
        {
            var rentalPrice = await _context.RentalPrices.FindAsync(id);
            if (rentalPrice == null) return NotFound();

            int houseId = rentalPrice.HouseId ?? 0;

            _context.RentalPrices.Remove(rentalPrice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { houseId });
        }










    }
}
