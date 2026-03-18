using Data.Data.HouseRentalData;
using HouseNet9.Controllers.Abstract.HouseNet9.Controllers.Admin;
using HouseNet9.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime;

namespace HouseNet9.Controllers
{
    public class HouseSettingsController : BaseAdminController
    {
        private readonly FileUploadService _fileUploadService;

        public HouseSettingsController(ApplicationDbContext context, FileUploadService fileUploadService, ILoggerFactory loggerFactory)
        :base(context, loggerFactory)
        {
            _fileUploadService = fileUploadService;
        }

        // =========================================
        // DETAILS – pobiera settings dla domu
        // =========================================
        public async Task<IActionResult> Details()
        {
            if (CurrentHouseId is not int houseId)
            {
                return RedirectToAction("Index", "Houses");
            }

            var settings = await GetSettingsForHouseAsync(houseId);
            //ViewBag.HouseId = CurrentHouseId;
                return View(settings);
        }

        // =========================================
        // CREATE – tworzenie nowych ustawień
        // =========================================
        public IActionResult Create()
        {
            //ViewBag.HouseId = CurrentHouseId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HouseSettings settings, IFormFile logoFile)
        {
            //tworzymy houseId
            if (CurrentHouseId is not int houseId)
            {
                return RedirectToAction("Index", "Houses");
            }

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

            return RedirectToAction(nameof(Details));
        }

        // =========================================
        // EDIT – edycja ustawień
        // =========================================
        public async Task<IActionResult> Edit(int id)
        {
            //tworzymy houseId
            if (CurrentHouseId is not int houseId)
            {
                return RedirectToAction("Index", "Houses");
            }

            var settings = await _context.HouseSettings
                .FirstOrDefaultAsync(s => s.Id == id);

            if (settings == null)
                return NotFound();

            ViewBag.HouseId = houseId;
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HouseSettings settings)
        {
            if (id != settings.Id)
                return BadRequest();


            var existing = await _context.HouseSettings
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existing == null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                //ViewBag.HouseId = houseId;
                return View(settings);
            }

            // aktualizacja zwykłych pól
            existing.DepositPercentage = settings.DepositPercentage;
            existing.DepositDueDays = settings.DepositDueDays;
            existing.FullPaymentDueDaysBeforeArrival = settings.FullPaymentDueDaysBeforeArrival;
            existing.BankAccountIban = settings.BankAccountIban;
            existing.BankAccountSwift = settings.BankAccountSwift;
            existing.BankAccountName = settings.BankAccountName;
            existing.BankName = settings.BankName;
            existing.Currency = settings.Currency;

            // pobranie pliku bez model bindera
            var logoFile = Request.Form.Files["logoFile"];

            if (logoFile != null && logoFile.Length > 0)
            {
                existing.LogoFileName = await _fileUploadService.EditFileAsync(
                    logoFile,
                    existing.LogoFileName
                );
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details));
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
        
        public async Task<IActionResult> RevertToDefault()
        {
            //tworzymy houseId
            if (CurrentHouseId is not int houseId)
            {
                return RedirectToAction("Index", "Houses");
            }

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

            return RedirectToAction(nameof(Details));
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