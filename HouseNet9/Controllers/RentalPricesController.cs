using Data.Data.HouseRentalData;
using HouseNet9.Controllers.Abstract.HouseNet9.Controllers.Admin;
using HouseNet9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers
{
    public class RentalPricesController : BaseAdminController
    {

        public RentalPricesController(ApplicationDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
        {
        }




        // INDEX – ceny dla konkretnego domu
        public async Task<IActionResult> Index()
        {

            var house = await _context.Houses.FindAsync(CurrentHouseId);
            if (house == null) return NotFound();

            ViewBag.House = house;

            var list = await _context.RentalPrices
                .Where(x => x.HouseId == CurrentHouseId)
                .OrderByDescending(x => x.DateTimeFrom)
                .ToListAsync();

            ViewBag.List = list;

            // model do formularza CREATE
            return View(new RentalPrice { HouseId = CurrentHouseId });
        }

        // CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalPrice rentalPrice)
        {
            if (rentalPrice.HouseId != CurrentHouseId)
                return NotFound();

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

            //return RedirectToAction(nameof(Index), new { CurrentHouseId });
            return RedirectToAction(nameof(Index));
        }










    }
}
