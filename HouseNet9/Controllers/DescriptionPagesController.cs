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
            return View(await _context.DescriptionPages.Include(i => i.Image).ToListAsync());
        }

        // GET: DescriptionPages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var descriptionPage = await _context.DescriptionPages
                .FirstOrDefaultAsync(m => m.DescriptionPageId == id);
            if (descriptionPage == null)
            {
                return NotFound();
            }

            return View(descriptionPage);
        }

        // GET: DescriptionPages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DescriptionPages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DescriptionPageId,Title,Description")] DescriptionPage descriptionPage, IFormFile file)
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
                        descriptionPage.Image = myFile;
                        var house = await _context.Houses.Include(i => i.DescriptionPages).FirstOrDefaultAsync();
                        if (house != null && house.DescriptionPages != null)
                        {
                            house.DescriptionPages.Add(descriptionPage);
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


            return RedirectToAction(nameof(Index));
        }

        // GET: DescriptionPages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var descriptionPage = await _context.DescriptionPages.Where(w => w.DescriptionPageId == id).Include(i => i.Image).FirstAsync();
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
        public async Task<IActionResult> Edit(int id, [Bind("DescriptionPageId,Title,Description")] DescriptionPage descriptionPage, IFormFile? file, string? ImagePath)
        {
            if (id != descriptionPage.DescriptionPageId)
                return NotFound();



            //var oldDesc = _context.DescriptionPages.Where(f => f.DescriptionPageId == id).Include(i => i.Image).FirstOrDefault();
            //if (oldDesc == null || oldDesc.Image == null)
            //    return NotFound();
            //oldDesc.Image.Path = await _fileUploadService.EditFileAsync(file, ImagePath);
            //oldDesc.Description = descriptionPage.Description;
            //oldDesc.Title = descriptionPage.Title;

            //dodaje obiejt image
            //descriptionPage.Image = new MyFile
            //{
            //    Path = await _fileUploadService.EditFileAsync(file, ImagePath)
            //};

            descriptionPage.Image = _context.MyFiles.Where(w => w.DescriptionPageId == descriptionPage.DescriptionPageId).FirstOrDefault();
            descriptionPage.Image.Path = await _fileUploadService.EditFileAsync(file, ImagePath);


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(descriptionPage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DescriptionPageExists(descriptionPage.DescriptionPageId))
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
            return View(descriptionPage);
        }

        // GET: DescriptionPages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var descriptionPage = await _context.DescriptionPages
                .FirstOrDefaultAsync(m => m.DescriptionPageId == id);
            if (descriptionPage == null)
            {
                return NotFound();
            }

            return View(descriptionPage);
        }

        // POST: DescriptionPages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var descriptionPage = await _context.DescriptionPages.Where(w => w.DescriptionPageId == id).Include(i => i.Image).FirstOrDefaultAsync();
            if (descriptionPage != null)
            {

                //null or empty
                _fileUploadService.DeleteFile(descriptionPage.Image?.Path?? "");
                _context.DescriptionPages.Remove(descriptionPage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DescriptionPageExists(int id)
        {
            return _context.DescriptionPages.Any(e => e.DescriptionPageId == id);
        }
    }
}
