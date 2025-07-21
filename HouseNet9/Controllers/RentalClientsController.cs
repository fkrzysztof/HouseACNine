using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data.HouseRentalData;
using HouseNet9.Data;

namespace HouseNet9.Controllers
{
    public class RentalClientsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RentalClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RentalClients
        public async Task<IActionResult> Index()
        {
            return View(await _context.RentalClients.ToListAsync());
        }

        // GET: RentalClients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalClient = await _context.RentalClients
                .FirstOrDefaultAsync(m => m.RentalClientId == id);
            if (rentalClient == null)
            {
                return NotFound();
            }

            return View(rentalClient);
        }

        // GET: RentalClients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RentalClients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RentalClientId,Name,LastName,Email,Phone,Country,City,Street,Number,ZIPCode")] RentalClient rentalClient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rentalClient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rentalClient);
        }

        // GET: RentalClients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalClient = await _context.RentalClients.FindAsync(id);
            if (rentalClient == null)
            {
                return NotFound();
            }
            return View(rentalClient);
        }

        // POST: RentalClients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RentalClientId,Name,LastName,Email,Phone,Country,City,Street,Number,ZIPCode")] RentalClient rentalClient)
        {
            if (id != rentalClient.RentalClientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rentalClient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalClientExists(rentalClient.RentalClientId))
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
            return View(rentalClient);
        }

        // GET: RentalClients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalClient = await _context.RentalClients
                .FirstOrDefaultAsync(m => m.RentalClientId == id);
            if (rentalClient == null)
            {
                return NotFound();
            }

            return View(rentalClient);
        }

        // POST: RentalClients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rentalClient = await _context.RentalClients.FindAsync(id);
            if (rentalClient != null)
            {
                _context.RentalClients.Remove(rentalClient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RentalClientExists(int id)
        {
            return _context.RentalClients.Any(e => e.RentalClientId == id);
        }
    }
}
