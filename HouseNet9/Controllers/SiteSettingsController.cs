using Data.Data.HouseRentalData;
using HouseNet9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers
{
    public class SiteSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SiteSettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUBLICZNA STRONA
        // /SiteSettings
        public async Task<IActionResult> Index()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SiteSettings
                {
                    PrivacyPolicy = "",
                    CookiesPolicy = ""
                };
            }

            return View(settings);
        }

        // PANEL ADMINA
        // /SiteSettings/Edit
        public async Task<IActionResult> Edit()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();

            // jeśli rekord nie istnieje -> utwórz pusty
            if (settings == null)
            {
                settings = new SiteSettings
                {
                    PrivacyPolicy = "",
                    CookiesPolicy = ""
                };

                _context.SiteSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return View(settings);
        }

        // POST: /SiteSettings/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SiteSettings model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var settings = await _context.SiteSettings.FirstOrDefaultAsync();

            // zabezpieczenie
            if (settings == null)
            {
                settings = new SiteSettings();
                _context.SiteSettings.Add(settings);
            }

            settings.PrivacyPolicy = model.PrivacyPolicy;
            settings.CookiesPolicy = model.CookiesPolicy;

            await _context.SaveChangesAsync();

            ViewBag.Success = true;

            return View(settings);
        }
    }
}
