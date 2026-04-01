using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data.HouseRentalData;
using HouseNet9.Data;
using HouseNet9.Controllers.Abstract.HouseNet9.Controllers.Admin;

namespace HouseNet9.Controllers
{
    public class HousesController : BaseAdminController
    {
        public HousesController(ApplicationDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
        {
        }

        // GET: Rules
        public async Task<IActionResult> Rules()
        {
            var house = await _context.Houses
                .FirstOrDefaultAsync(h => h.HouseId == CurrentHouseId);

            return View(house);
        }

        // GET: Houses
        public async Task<IActionResult> Index()
        {
            // return View(await _context.Houses.Where(w => w.IsActive).ToListAsync());
            var houses = _context.Houses
        .Include(h => h.DescriptionPages)
            .ThenInclude(dp => dp.Images)
        .ToList();
            return View(houses);

        }

        //SelectHouse
        ///przekierowanie do edycji 
        //+ zapis/aktualizacja sesji
        public async Task<IActionResult> SelectHouse(int id)
        {
            var house = await _context.Houses.FindAsync(id);
            if (house == null)
            {
                return NotFound();
            }

            // ustawiam sesję
            HttpContext.Session.SetInt32("AdminCurrentHouseId", house.HouseId);
            HttpContext.Session.SetString("AdminCurrentHouseName", house.Name);

            // przekierowanie do edit
            return RedirectToAction("Edit");
        }

        // GET: Houses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Houses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HouseId,Name,ShortText,LongText,RentalRules,IsActive")] House house)
        {
            if (ModelState.IsValid)
            {
                _context.Add(house);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(house);
        }

        public async Task<IActionResult> Edit()
        {
            if(CurrentHouseId == null)
                return RedirectToAction(nameof(Index));

            var house = await _context.Houses.FindAsync(CurrentHouseId);
            if (house == null)
            {
                return NotFound();
            }

            return View(house);
        }

        // POST: Houses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HouseId,Name,ShortText,LongText,RentalRules,IsActive")] House house)
        {
            if (id != house.HouseId || house.HouseId != CurrentHouseId.Value)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(house);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HouseExists(house.HouseId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Edit), new { id = CurrentHouseId });
            }
            return View(house);
        }

        // GET: Houses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var house = await _context.Houses
                .FirstOrDefaultAsync(m => m.HouseId == id);
            if (house == null)
            {
                return NotFound();
            }

            return View(house);
        }

        // POST: Houses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var house = await _context.Houses.FindAsync(id);
            if (house != null)
            {
                _context.Houses.Remove(house);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HouseExists(int id)
        {
            return _context.Houses.Any(e => e.HouseId == id);
        }
    }
}
