using Data.Data.HouseRentalData;
using HouseNet9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers
{
    public class DistanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUploadService _fileUploadService;

        public DistanceController(ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public IActionResult Index()
        {

            return View(_context.Distances.Include(i => i.Image).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DistanceID,Name,FormFileItem")] Distance distance)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var filePath = await _fileUploadService.UploadFileAsync(distance.FormFileItem);
                    if (filePath != null)
                    {

                        MyFile myFile = new MyFile();
                        myFile.Path = filePath;
                        distance.Image = myFile;
                        var house = await _context.Houses.Include(i => i.Distances).FirstOrDefaultAsync();
                        if (house != null && house.Distances != null)
                        {
                            house.Distances.Add(distance);
                            await _context.SaveChangesAsync();

                        }

                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The process failed: {0}", e.ToString());
                }

            }

            return View(distance);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var distance = await _context.Distances
                .Where(i => i.DistanceID == id)
                .Include(i => i.Image)
                .FirstOrDefaultAsync();

            if (distance == null)
            {
                return NotFound();
            }
            return View(distance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DistanceID,Name,FormFileItem")] Distance distance, string? ImagePath)
        {
            if (id != distance.DistanceID)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    distance.Image = _context.MyFiles.Where(w => w.DistanceID == distance.DistanceID).FirstOrDefault();
                    if (distance.Image == null)
                    {
                        distance.Image = new MyFile() { DetailedInformationId = id };
                    }
                    distance.Image.Path = await _fileUploadService.EditFileAsync(distance.FormFileItem, ImagePath);


                    _context.Update(distance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DistanceExists(distance.DistanceID))
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
            return View(distance);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var distance = await _context.Distances
                .Where(w => w.DistanceID == id)
                .Include(i => i.Image)
                .Include(i => i.DistanceItems)
                .FirstOrDefaultAsync();

            if (distance == null)
            {
                return NotFound();
            }

            return View(distance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDistanceItem([Bind("DistanceItemId,NameInfo,DistanceInfo")] DistanceItem distanceItem, int id)
        {
            var distance = await _context.Distances
                .Where(w => w.DistanceID == id)
                .Include(i => i.DistanceItems)
                .FirstOrDefaultAsync();

            if (distance == null)
                return NotFound();


            if (ModelState.IsValid)
            {

                distance.DistanceItems.Add(distanceItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id });

            }
            return NotFound();
        }

        public async Task<IActionResult> DeleteDistanceItem(int distanceItemId, int id)
        {
            var distanceItem = await _context.DistanceItems.FirstOrDefaultAsync(f => f.DistanceItemId == distanceItemId);
            if (distanceItem != null)
            {
                _context.DistanceItems.Remove(distanceItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var distance = await _context.Distances
                .Where(w => w.DistanceID == id)
                .Include(i => i.Image)
                .Include(i => i.DistanceItems)
                .FirstOrDefaultAsync();

            if (distance != null)
            {
                if (distance.DistanceItems != null)
                {
                    _context.DistanceItems.RemoveRange(distance.DistanceItems);
                }

                bool result = _fileUploadService.DeleteFile(distance.Image.Path);
                if (!result)
                {
                    return Ok("bląd");
                }
                else
                {
                    _context.MyFiles.Remove(distance.Image);
                    _context.Distances.Remove(distance);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DistanceExists(int id)
        {
            return _context.Distances.Any(e => e.DistanceID == id);
        }

    }
}
