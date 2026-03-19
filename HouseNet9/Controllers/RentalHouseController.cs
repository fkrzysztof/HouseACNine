using Data.Data.HouseRentalData;
using HouseNet9.Controllers.Abstract.HouseNet9.Controllers.Admin;
using HouseNet9.Data;
using HouseNet9.Helpers;
using HouseNet9.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers
{
    public class RentalHouseController : BaseAdminController
    {
        private readonly RentalCalculatorService _calculator;
        private readonly IReservationNotificationService _notificationService;

        public RentalHouseController(ApplicationDbContext context, IReservationNotificationService reservationNotificationService, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
        {
            _calculator = new RentalCalculatorService(_context);
            _notificationService = reservationNotificationService;
        }


        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string? clientSearch, string? showStatus)
        {
            var query = _context.RentalHouses
                .Where(w => w.HouseId == CurrentHouseId)
                .Include(r => r.House)
                .Include(r => r.RentalClient)
                .Include(r => r.RentalStatus)
                //.Where(r => r.IsActive == true)
                .AsQueryable();

            // Filtr statusu
            switch (showStatus)
            {
                case "active":
                    query = query.Where(r => r.IsActive == true);
                    break;
                case "inactive":
                    query = query.Where(r => r.IsActive == false);
                    break;
                case "all":
                default:
                    // brak filtra - wszystkie
                    break;
            }

            ViewData["ShowStatus"] = showStatus ?? "active"; // domyślnie aktywne

            ViewBag.Statuses = await _context.RentalStatus
                                .OrderBy(s => s.RentalStatusID)
                                .ToListAsync();


            if (fromDate.HasValue)
                query = query.Where(r => r.From >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(r => r.To <= toDate.Value);

            //  Wyszukiwanie po fragmencie nazwiska imienia
            if (!string.IsNullOrWhiteSpace(clientSearch))
            {
                query = query.Where(r =>
                (r.RentalClient.Name + " " + r.RentalClient.LastName)
                .Contains(clientSearch));
            }

            query = query.OrderByDescending(r => r.CreationDate);

            var house = await _context.Houses.FirstOrDefaultAsync(h => h.HouseId == CurrentHouseId);
            if (house != null)
            {
                var settings = house.HouseSettingsId != null ? await _context.HouseSettings.FirstOrDefaultAsync(s => s.Id == house.HouseSettingsId)
                                                 : await _context.HouseSettings.FirstOrDefaultAsync(s => s.IsDefault);
                ViewBag.Currency = settings?.Currency;
            }
            ViewData["ClientSearch"] = clientSearch;
            ViewData["SumPrice"] = query.Sum(p => p.ToPay);

            return View(await query.ToListAsync());
        }




        // GET: Create
        public IActionResult Create()
        {
            if (CurrentHouseId == null)
                return RedirectToAction("Index", "House");

            var model = new RentalHouse
            {
                HouseId = CurrentHouseId,
                From = DateTime.Today,
                To = DateTime.Today.AddDays(6) // domyślnie 6 dni
            };

            return View(model);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalHouse rentalHouse)
        {

            if (CurrentHouseId == null)
                return RedirectToAction("Index", "House");

            rentalHouse.HouseId = CurrentHouseId;
           // rentalHouse.ReservationNumber = ReservationNumberGenerator.Generate();
            rentalHouse.CreationDate = DateTime.Now;
            rentalHouse.IsActive = true;
            //klient
            rentalHouse.RentalClientId = null;
            rentalHouse.RentalClient = null;
            //platnosci
            rentalHouse.ToPay = 0;
            rentalHouse.RentalStatusID = 8; //wynajem własny
            rentalHouse.DepositAmount = 0;
            rentalHouse.DepositDueDate = null;
            rentalHouse.RemainingAmount = 0;
            rentalHouse.RemainingDueDate = null;
            rentalHouse.DepositPaidDate = null;
            rentalHouse.RemainingPaidDate = null;
            rentalHouse.DepositReminderSent = true;
            rentalHouse.RemainingReminderSent = false;


            // 🔹 Walidacja dat
            if (rentalHouse.To <= rentalHouse.From)
            {
                ModelState.AddModelError("", "Data zakończenia musi być późniejsza niż data rozpoczęcia.");
                return View(rentalHouse);
            }

            // 🔹 Sprawdzenie dostępności terminu
            bool isOccupied = await _context.RentalHouses
                .AnyAsync(r => r.HouseId == rentalHouse.HouseId
                            && r.IsActive
                            && r.From <= rentalHouse.To   // istniejąca rezerwacja zaczyna się przed końcem nowej
                            && r.To >= rentalHouse.From); // istniejąca rezerwacja kończy się po początku nowej


            if (isOccupied)
            {
                ModelState.AddModelError("", "Wybrany termin jest już zajęty.");
                return View(rentalHouse);
            }

            ModelState.Remove(nameof(RentalHouse.ReservationNumber));
            if (ModelState.IsValid)
            {

                bool saved = false;

                while (!saved)
                {
                    try
                    {
                        rentalHouse.ReservationNumber = ReservationNumberGenerator.Generate();

                        _context.RentalHouses.Add(rentalHouse);
                        await _context.SaveChangesAsync();

                        saved = true;
                    }
                    catch (DbUpdateException)
                    {
                        // kolizja – próbujemy jeszcze raz
                        _context.Entry(rentalHouse).State = EntityState.Detached;
                    }
                }

                return RedirectToAction(nameof(Index));
               
            }
            return View(rentalHouse);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatusSelect([FromBody] StatusChangeModel model)
        {
            var rental = await _context.RentalHouses
                .Include(r => r.RentalStatus)
                .Include(r => r.House)
                .ThenInclude(h => h.Settings)
                .Include(r => r.RentalClient)
                .FirstOrDefaultAsync(r => r.RentalHouseID == model.Id);

            if (rental == null) return NotFound();

            var status = await _context.RentalStatus
                .FirstOrDefaultAsync(s => s.RentalStatusID == model.StatusId);

            if (status == null) return NotFound();


            rental.RentalStatusID = status.RentalStatusID;
            //dodanie informaci do adnotacji
            rental.Annotations += $"{DateTime.Now:yyyy-MM-dd HH:mm} - zmiana statusu na: {status.Name}\n";
            await _context.SaveChangesAsync();

            // Flaga informująca, czy wysłano mail
            bool emailSent = false;

            //wysylanie maila
            if (status.RentalStatusID == 6) // Wpłacono zaliczkę
            {
                rental.DepositPaidDate = DateTime.Now;
                await _notificationService.SendDepositConfirmedAsync(rental);
                
                emailSent = true;
            }
            else if (status.RentalStatusID == 1) // Zapłacono całość
            {
                rental.DepositPaidDate ??= DateTime.Now;
                rental.RemainingPaidDate = DateTime.Now;
                await _notificationService.SendFullPaymentConfirmedAsync(rental);
                emailSent = true;
            }

            string badgeColor = status.Color;

            return Json(new { statusId = status.RentalStatusID, badgeColor, emailSent });
        }

        // Model dla AJAX
        public class StatusChangeModel
        {
            public int Id { get; set; }
            public int StatusId { get; set; }
        }



        // POST: RentalHouse/Delete/5    IsActive 1/0
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var rentalHouse = await _context.RentalHouses.FindAsync(id);
            if (rentalHouse == null) return NotFound();

            rentalHouse.Annotations += $"{DateTime.Now:yyyy-MM-dd HH:mm} - usunięto rezerwację\n";

            rentalHouse.IsActive = false;
            _context.Update(rentalHouse);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Restore/Delete    IsActive 1/0
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var rental = await _context.RentalHouses.FindAsync(id);
            if (rental != null)
            {
                rental.Annotations += $"{DateTime.Now:yyyy-MM-dd HH:mm} - przywrócono rezerwację\n";
                rental.IsActive = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RentalHouseExists(int id)
        {
            return _context.RentalHouses.Any(e => e.RentalHouseID == id);
        }


    }


}
