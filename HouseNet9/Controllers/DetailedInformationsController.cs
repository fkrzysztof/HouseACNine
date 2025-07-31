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
    public class DetailedInformationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUploadService _fileUploadService;

        public DetailedInformationsController(ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // GET: DetailedInformations
        public async Task<IActionResult> Index()
        {
            return View(await _context.DetailedInformation.Include(i => i.Image).ToListAsync());
        }

        // GET: DetailedInformations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailedInformation = await _context.DetailedInformation
                .Where(w => w.DetailedInformationId == id)
                .Include(i => i.Image)
                .Include(i => i.DetailedInformationItems)
                .FirstOrDefaultAsync();

            if (detailedInformation == null)
            {
                return NotFound();
            }

            return View(detailedInformation);
        }

        // GET: DetailedInformations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DetailedInformations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DetailedInformationId,Name")] DetailedInformation detailedInformation, IFormFile file)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var filePath = await _fileUploadService.UploadFileAsync(file);
                    if (filePath != null)
                    {

                        MyFile myFile = new MyFile();
                        myFile.Path = filePath;
                        detailedInformation.Image = myFile;
                        var house = await _context.Houses.Include(i => i.DetailedInformation).FirstOrDefaultAsync();
                        if (house != null && house.DetailedInformation != null)
                        {
                            house.DetailedInformation.Add(detailedInformation);
                            await _context.SaveChangesAsync();

                        }

                        ViewData["Message"] = $"Plik '{file.FileName}' został przesłany.";
                        return View("Create");


                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());
                }

            }

            return View(detailedInformation);
        }

        // GET: DetailedInformations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailedInformation = await _context.DetailedInformation
                .Where(i => i.DetailedInformationId == id)
                .Include(i => i.Image)
                .FirstOrDefaultAsync();

            if (detailedInformation == null)
            {
                return NotFound();
            }
            return View(detailedInformation);
        }

        // POST: DetailedInformations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DetailedInformationId,Name")] DetailedInformation detailedInformation, IFormFile? file, string? ImagePath)
        {
            if (id != detailedInformation.DetailedInformationId)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    detailedInformation.Image = _context.MyFiles.Where(w => w.DetailedInformationId == detailedInformation.DetailedInformationId).FirstOrDefault();
                    if (detailedInformation.Image == null)
                    {
                        detailedInformation.Image =  new MyFile() { DetailedInformationId = id};
                    }
                    detailedInformation.Image.Path = await _fileUploadService.EditFileAsync(file, ImagePath);
                     

                    _context.Update(detailedInformation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetailedInformationExists(detailedInformation.DetailedInformationId))
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
            return View(detailedInformation);
        }

        // GET: DetailedInformations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detailedInformation = await _context.DetailedInformation
                .FirstOrDefaultAsync(m => m.DetailedInformationId == id);
            if (detailedInformation == null)
            {
                return NotFound();
            }

            return View(detailedInformation);
        }

        // POST: DetailedInformations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detailedInformation = await _context.DetailedInformation.FindAsync(id);
            if (detailedInformation != null)
            {
                _context.DetailedInformation.Remove(detailedInformation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> DeleteDetailedInformationItem(int DetailedInformationItemId, int DetailedInformationItem)
        {
            var detailedInformationItem = await _context.DetailedInformationItems.FirstOrDefaultAsync(f =>  f.DetailedInformationItemId == DetailedInformationItemId);
            if (detailedInformationItem != null)
            {
                _context.DetailedInformationItems.Remove(detailedInformationItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { DetailedInformationItem });
        }


        // POST: DetailedInformationItems/Create DetailedInformationItems
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDetailedInformationItems([Bind("DetailedInformationItemId,Description")] DetailedInformationItem detailedInformationItem, int id)
        {
            var detailedInfo = await _context.DetailedInformation
                .Where(w => w.DetailedInformationId == id)
                .Include(i => i.DetailedInformationItems)
                .FirstOrDefaultAsync();

            if (detailedInfo == null)
                return NotFound();
            

                if (ModelState.IsValid)
            {

                    detailedInfo.DetailedInformationItems.Add(detailedInformationItem);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id });

            }
            return NotFound();
        }

        private bool DetailedInformationExists(int id)
        {
            return _context.DetailedInformation.Any(e => e.DetailedInformationId == id);
        }
    }
}
