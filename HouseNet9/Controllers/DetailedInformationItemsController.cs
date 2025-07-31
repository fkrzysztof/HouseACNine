using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data.HouseRentalData;
using HouseNet9.Data;

namespace HouseNet9.Controllers
{
    public class DetailedInformationItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetailedInformationItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DetailedInformationItems
        public async Task<IActionResult> Index()
        {
            return View(await _context.DetailedInformationItems.ToListAsync());
        }

        // GET: DetailedInformationItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailedInformationItem = await _context.DetailedInformationItems
                .FirstOrDefaultAsync(m => m.DetailedInformationItemId == id);
            if (detailedInformationItem == null)
            {
                return NotFound();
            }

            return View(detailedInformationItem);
        }

        // GET: DetailedInformationItems/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DetailedInformationItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DetailedInformationItemId,Description")] DetailedInformationItem detailedInformationItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detailedInformationItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(detailedInformationItem);
        }

        // GET: DetailedInformationItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailedInformationItem = await _context.DetailedInformationItems.FindAsync(id);
            if (detailedInformationItem == null)
            {
                return NotFound();
            }
            return View(detailedInformationItem);
        }

        // POST: DetailedInformationItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DetailedInformationItemId,Description")] DetailedInformationItem detailedInformationItem)
        {
            if (id != detailedInformationItem.DetailedInformationItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detailedInformationItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetailedInformationItemExists(detailedInformationItem.DetailedInformationItemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(detailedInformationItem);
        }

        // GET: DetailedInformationItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailedInformationItem = await _context.DetailedInformationItems
                .FirstOrDefaultAsync(m => m.DetailedInformationItemId == id);
            if (detailedInformationItem == null)
            {
                return NotFound();
            }

            return View(detailedInformationItem);
        }

        // POST: DetailedInformationItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detailedInformationItem = await _context.DetailedInformationItems.FindAsync(id);
            if (detailedInformationItem != null)
            {
                _context.DetailedInformationItems.Remove(detailedInformationItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetailedInformationItemExists(int id)
        {
            return _context.DetailedInformationItems.Any(e => e.DetailedInformationItemId == id);
        }
    }
}
