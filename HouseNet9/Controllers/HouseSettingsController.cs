using Data.Data.HouseRentalData;
using HouseNet9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace HouseNet9.Controllers
{
    public class HouseSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUploadService _fileUploadService;

        public HouseSettingsController(ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // =========================================
        // DETAILS – pobiera settings dla domu
        // =========================================
        public async Task<IActionResult> Details(int houseId)
        {
            var settings = await GetSettingsForHouseAsync(houseId);
            ViewBag.HouseId = houseId;
            return View(settings);
        }

        // =========================================
        // CREATE – tworzenie nowych ustawień
        // =========================================
        public IActionResult Create(int houseId)
        {
            ViewBag.HouseId = houseId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int houseId, HouseSettings settings, IFormFile logoFile)
        {
            settings.IsDefault = false;
            if (!ModelState.IsValid)
                return View(settings);

            // przesyłanie logo
            if (logoFile != null)
            {
                var fileName = await _fileUploadService.UploadFileAsync(logoFile);
                settings.LogoFileName = fileName;
            }

            _context.HouseSettings.Add(settings);
            await _context.SaveChangesAsync();

            // przypisanie ustawień do domu
            var house = await _context.Houses.FindAsync(houseId);
            if (house != null)
            {
                house.HouseSettingsId = settings.Id;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { houseId });
        }

        // =========================================
        // EDIT – edycja ustawień
        // =========================================
        public async Task<IActionResult> Edit(int id, int houseId)
        {
            var settings = await _context.HouseSettings
                .FirstOrDefaultAsync(s => s.Id == id);

            if (settings == null)
                return NotFound();

            ViewBag.HouseId = houseId;
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int houseId, HouseSettings settings, IFormFile logoFile)
        {
            if (id != settings.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(settings);

            // aktualizacja logo, jeśli przesłano nowy plik
            if (logoFile != null)
            {
                settings.LogoFileName = await _fileUploadService.EditFileAsync(logoFile, settings.LogoFileName);
            }

            _context.Update(settings);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { houseId });
        }

        // =========================================
        // SET DEFAULT – ustawia jedno jako domyślne
        // =========================================
        public async Task<IActionResult> SetDefault(int id)
        {
            var settings = await _context.HouseSettings.FindAsync(id);
            if (settings == null)
                return NotFound();

            // wyłącz wszystkie inne
            await _context.HouseSettings
                .Where(s => s.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDefault, false));

            // ustaw nowe
            settings.IsDefault = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // INDEX – lista wszystkich settings
        // =========================================
        //public async Task<IActionResult> Index()
        //{
        //    var list = await _context.HouseSettings.ToListAsync();
        //    return View(list);
        //}


        // =========================================
        // REVERT TO DEFAULT – przywraca domyślne ustawienia dla domu
        // =========================================
        public async Task<IActionResult> RevertToDefault(int houseId)
        {
            var house = await _context.Houses
                .Include(h => h.Settings)
                .FirstOrDefaultAsync(h => h.HouseId == houseId);

            if (house == null)
                return NotFound();

            if (house.Settings != null && !house.Settings.IsDefault)
            {
                // 🔹 usuń plik logo jeśli istnieje
                if (!string.IsNullOrWhiteSpace(house.Settings.LogoFileName))
                {
                    _fileUploadService.DeleteFile(house.Settings.LogoFileName);
                }

                // 🔹 usuń rekord z bazy
                _context.HouseSettings.Remove(house.Settings);

                // 🔹 odłącz settings od domu
                house.HouseSettingsId = null;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { houseId });
        }

        // =========================================
        // POMOCNICZA – pobiera ustawienia dla domu
        // =========================================
        private async Task<HouseSettings> GetSettingsForHouseAsync(int houseId)
        {
            var house = await _context.Houses
                .Include(h => h.Settings)
                .FirstOrDefaultAsync(h => h.HouseId == houseId);

            if (house == null)
                throw new Exception("Nie znaleziono domu.");

            // dom ma przypisane settings
            if (house.Settings != null)
                return house.Settings;

            // istnieje default
            var defaultSettings = await _context.HouseSettings
                .FirstOrDefaultAsync(s => s.IsDefault);

            if (defaultSettings != null)
                return defaultSettings;

            // brak ustawień – tworzymy automatyczny default
            var newDefault = new HouseSettings
            {
                DepositPercentage = 30,
                DepositDueDays = 3,
                FullPaymentDueDaysBeforeArrival = 7,
                Currency = "€",
                IsDefault = true
            };

            _context.HouseSettings.Add(newDefault);
            await _context.SaveChangesAsync();

            return newDefault;
        }
    }
}