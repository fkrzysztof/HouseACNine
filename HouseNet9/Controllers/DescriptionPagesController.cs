using Data.Data.HouseRentalData;
using HouseNet9.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HouseNet9.Controllers
{
    public class DescriptionPagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUploadService _fileUploadService;

        public DescriptionPagesController(ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // GET: DescriptionPages
        public async Task<IActionResult> Index()
        {
            return View(await _context.DescriptionPages.Include(i => i.Images).ToListAsync());
        }



        // GET: DescriptionPages/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DescriptionPageId,Title,Description")] DescriptionPage descriptionPage,List<IFormFile> files)
        {
            if (!ModelState.IsValid)
                return View(descriptionPage);

            try
            {
                var house = await _context.Houses
                    .Include(h => h.DescriptionPages)
                    .FirstOrDefaultAsync();

                if (house == null)
                {
                    ModelState.AddModelError("", "Nie znaleziono domu.");
                    return View(descriptionPage);
                }

                // Zapis DescriptionPage aby miała ID
                house.DescriptionPages.Add(descriptionPage);
                await _context.SaveChangesAsync();

                // --- ZAPIS WIELU PLIKÓW ---
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        var filePath = await _fileUploadService.UploadFileAsync(file);

                        if (filePath != null)
                        {
                            var myFile = new MyFile
                            {
                                Path = filePath,
                                DescriptionPageId = descriptionPage.DescriptionPageId
                            };

                            descriptionPage.Images.Add(myFile);
                            _context.MyFiles.Add(myFile);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
                ModelState.AddModelError("", "Wystąpił błąd podczas zapisu.");
                return View(descriptionPage);
            }
        }


        // GET: DescriptionPages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var descriptionPage = await _context.DescriptionPages.Where(w => w.DescriptionPageId == id).Include(i => i.Images).FirstAsync();
            if (descriptionPage == null)
            {
                return NotFound();
            }
            return View(descriptionPage);
        }

        // POST: DescriptionPages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
      int id,
      [Bind("DescriptionPageId,Title,Description")] DescriptionPage descriptionPage,
      List<IFormFile> files)  // <- zmienione na List<IFormFile>
        {
            if (id != descriptionPage.DescriptionPageId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(descriptionPage);

            try
            {
                // Pobierz istniejącą stronę z bazy wraz z kolekcją Images
                var existingPage = await _context.DescriptionPages
                    .Include(d => d.Images)
                    .FirstOrDefaultAsync(d => d.DescriptionPageId == id);

                if (existingPage == null)
                    return NotFound();

                // Aktualizacja pól
                existingPage.Title = descriptionPage.Title;
                existingPage.Description = descriptionPage.Description;

                // --- Obsługa nowych plików ---
                if (files != null && files.Any())
                {
                    foreach (var file in files)
                    {
                        var filePath = await _fileUploadService.UploadFileAsync(file);
                        if (filePath != null)
                        {
                            var myFile = new MyFile
                            {
                                Path = filePath,
                                DescriptionPageId = existingPage.DescriptionPageId
                            };

                            existingPage.Images.Add(myFile);
                            _context.MyFiles.Add(myFile);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DescriptionPages.Any(d => d.DescriptionPageId == id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Błąd: {0}", e);
                ModelState.AddModelError("", "Wystąpił błąd podczas zapisu.");
                return View(descriptionPage);
            }
        }


     

        private bool DescriptionPageExists(int id)
        {
            return _context.DescriptionPages.Any(e => e.DescriptionPageId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.MyFiles.FindAsync(id);
            if (image == null) return Json(new { success = false });

            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", image.Path);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                _context.MyFiles.Remove(image);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var page = await _context.DescriptionPages
                .Include(p => p.Images)
                .FirstOrDefaultAsync(m => m.DescriptionPageId == id);

            if (page == null)
                return Json(new { success = false });

            // Usuń pliki fizyczne
            if (page.Images != null)
            {
                foreach (var img in page.Images)
                {
                    if (!string.IsNullOrEmpty(img.Path))
                    {
                        string fullPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot/uploads",
                            img.Path
                        );

                        if (System.IO.File.Exists(fullPath))
                            System.IO.File.Delete(fullPath);
                    }
                }
            }

            // Usuń rekordy zdjęć
            _context.MyFiles.RemoveRange(page.Images);

            // Usuń DescriptionPage
            _context.DescriptionPages.Remove(page);

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


    }
}
