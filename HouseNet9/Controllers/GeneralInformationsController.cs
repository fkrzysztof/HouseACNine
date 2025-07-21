using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Data.HouseRentalData;
using HouseNet9.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HouseNet9.Controllers
{
    public class GeneralInformationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUploadService _fileUploadService;

        public GeneralInformationsController(ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // GET: GeneralInformations
        public IActionResult Index()
        {

            return View(_context.GeneralInformation.ToList());
        }

        // GET: GeneralInformations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generalInformation = await _context.GeneralInformation
                .FirstOrDefaultAsync(m => m.GeneralInformationId == id);
            if (generalInformation == null)
            {
                return NotFound();
            }

            return View(generalInformation);
        }

        // GET: GeneralInformations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GeneralInformations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("GeneralInformationId,Name")] GeneralInformation generalInformation)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(generalInformation);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(generalInformation);
        //}



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, [Bind("GeneralInformationId,Name")] GeneralInformation generalInformation)
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
                        generalInformation.Image = myFile;
                        var house = await _context.Houses.Include(i => i.GeneralInformation).FirstOrDefaultAsync();
                        if (house != null && house.GeneralInformation != null)
                        {
                            house.GeneralInformation.Add(generalInformation);
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


        // GET: GeneralInformations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generalInformation = await _context.GeneralInformation.FindAsync(id);
            if (generalInformation == null)
            {
                return NotFound();
            }
            return View(generalInformation);
        }

        // POST: GeneralInformations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GeneralInformationId,Name")] GeneralInformation generalInformation)
        {
            if (id != generalInformation.GeneralInformationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(generalInformation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeneralInformationExists(generalInformation.GeneralInformationId))
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
            return View(generalInformation);
        }

        // GET: GeneralInformations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var generalInformation = await _context.GeneralInformation
                .FirstOrDefaultAsync(m => m.GeneralInformationId == id);
            if (generalInformation == null)
            {
                return NotFound();
            }

            return View(generalInformation);
        }

        // POST: GeneralInformations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var generalInformation = await _context.GeneralInformation.Include(i => i.Image).FirstOrDefaultAsync(f => f.GeneralInformationId == id);
            if (generalInformation != null)
            {

                if (generalInformation.Image != null && generalInformation.Image.Path != null)
                {
                    bool result = _fileUploadService.DeleteFile(generalInformation.Image.Path);
                    if (!result)
                    {
                        return Ok("bląd");
                    }
                    else
                    {
                        _context.MyFiles.Remove(generalInformation.Image);
                        _context.GeneralInformation.Remove(generalInformation);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool GeneralInformationExists(int id)
        {
            return _context.GeneralInformation.Any(e => e.GeneralInformationId == id);
        }
    }
}
