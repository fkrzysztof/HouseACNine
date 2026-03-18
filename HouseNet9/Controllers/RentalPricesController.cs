using Data.Data.HouseRentalData;
using HouseNet9.Controllers.Abstract.HouseNet9.Controllers.Admin;
using HouseNet9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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

            //ViewBag.House = house;

            var list = await _context.RentalPrices
                .Where(x => x.HouseId == CurrentHouseId && x.IsActive)
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

                var list = await _context.RentalPrices
                                .Where(x => x.HouseId == CurrentHouseId)
                                .OrderByDescending(x => x.DateTimeFrom)
                                .ToListAsync();
                ViewBag.List = list;
                return RedirectToAction(nameof(Index));
            }

            return View(rentalPrice);
        }

        // EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var rentalPrice = await _context.RentalPrices.FindAsync(id);
            if (rentalPrice == null) return NotFound();

            return View(rentalPrice);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RentalPrice rentalPrice)
        {
            rentalPrice.HouseId = CurrentHouseId;
            if (id != rentalPrice.RentalPriceID)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(rentalPrice);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(rentalPrice);
        }

        // DELETE   IsActive 1/0
        public async Task<IActionResult> Delete(int id)
        {
            var rentalPrice = await _context.RentalPrices.FindAsync(id);
            if (rentalPrice == null) 
                return NotFound();

            rentalPrice.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
