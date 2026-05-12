using Data.Data.HouseRentalData;
using HouseNet9.Controllers.Abstract.HouseNet9.Controllers.Admin;
using HouseNet9.Data;
using HouseNet9.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HouseNet9.Controllers
{
    public class ContactsController : BaseAdminController
    {

        public ContactsController(ApplicationDbContext context, ILoggerFactory loggerFactory)
        : base(context, loggerFactory)
        {
        }

        // GET: Contacts
        public async Task<IActionResult> Index()
        {
            var contact = await _context.Contacts
                .Where(w => w.HouseId == CurrentHouseId)
                .Include(i => i.Addresses)
                .Include(i => i.EmailAddresses)
                .Include(i => i.PhoneNumbers)
                .ToArrayAsync();
            return View(contact);
        }


        // GET: Contacts/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contact model)
        {
            var houseId = CurrentHouseId;
            if (!houseId.HasValue)
                return BadRequest("Nie wybrano domu.");

            model.HouseId = houseId.Value;

            // zabezpieczenie null (WAŻNE)
            model.Addresses ??= new();
            model.EmailAddresses ??= new();
            model.PhoneNumbers ??= new();

            // filtrujemy puste rekordy (tak jak w Edit)
            model.Addresses = model.Addresses
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Street) ||
                    !string.IsNullOrWhiteSpace(x.City) ||
                    !string.IsNullOrWhiteSpace(x.PostalCode) ||
                    !string.IsNullOrWhiteSpace(x.Country))
                .ToList();

            model.EmailAddresses = model.EmailAddresses
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .ToList();

            model.PhoneNumbers = model.PhoneNumbers
                .Where(x => !string.IsNullOrWhiteSpace(x.Number))
                .ToList();

            _context.Contacts.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        //GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var contact = await _context.Contacts
                .Include(c => c.Addresses)
                .Include(c => c.EmailAddresses)
                .Include(c => c.PhoneNumbers)
                .FirstOrDefaultAsync(c => c.ContactId == id);

            if (contact == null)
                return NotFound();

            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Contact model, string submitType)
        {
            if (model.ContactId == 0)
                return BadRequest();

            if (submitType == "delete")
            {
                await deleteContact(model.ContactId);
                return RedirectToAction(nameof(Index));
            }

            var contactInDb = await _context.Contacts
                .Include(c => c.Addresses)
                .Include(c => c.EmailAddresses)
                .Include(c => c.PhoneNumbers)
                .FirstOrDefaultAsync(c => c.ContactId == model.ContactId);

            if (contactInDb == null)
                return NotFound();

            // =====================
            // BASIC DATA
            // =====================
            contactInDb.Name = model.Name;

            if (CurrentHouseId.HasValue)
                contactInDb.HouseId = CurrentHouseId.Value;

            // zabezpieczenie null
            model.Addresses ??= new();
            model.EmailAddresses ??= new();
            model.PhoneNumbers ??= new();



            // =========================================================
            // ADDRESSES
            // =========================================================

            // usuń usunięte adresy
            var addressIds = model.Addresses
                .Where(x => x.AddressId > 0)
                .Select(x => x.AddressId)
                .ToList();

            foreach (var existingAddress in contactInDb.Addresses.ToList())
            {
                if (!addressIds.Contains(existingAddress.AddressId))
                {
                    _context.Addresses.Remove(existingAddress);
                }
            }

            // aktualizacja + dodawanie
            foreach (var a in model.Addresses.Where(x =>
                !string.IsNullOrWhiteSpace(x.Street) ||
                !string.IsNullOrWhiteSpace(x.City) ||
                !string.IsNullOrWhiteSpace(x.PostalCode) ||
                !string.IsNullOrWhiteSpace(x.Country)))
            {
                var existing = contactInDb.Addresses
                    .FirstOrDefault(x => x.AddressId == a.AddressId);

                if (existing != null)
                {
                    // UPDATE
                    existing.Street = a.Street;
                    existing.City = a.City;
                    existing.PostalCode = a.PostalCode;
                    existing.Country = a.Country;
                    //existing.IsHouseAddress = a.IsHouseAddress;
                }
                else
                {
                    // ADD NEW
                    contactInDb.Addresses.Add(new Address
                    {
                        Street = a.Street,
                        City = a.City,
                        PostalCode = a.PostalCode,
                        Country = a.Country,
                        IsHouseAddress = false,
                        ContactId = contactInDb.ContactId
                    });
                }
            }

            // =========================================================
            // EMAILS
            // =========================================================
            _context.EmailAddresses.RemoveRange(contactInDb.EmailAddresses);

            contactInDb.EmailAddresses = model.EmailAddresses
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .Select(e => new EmailAddress
                {
                    EmailAddressId = e.EmailAddressId > 0 ? e.EmailAddressId : 0,
                    Email = e.Email,
                    ContactId = contactInDb.ContactId
                })
                .ToList();

            // =========================================================
            // PHONES
            // =========================================================
            _context.PhoneNumbers.RemoveRange(contactInDb.PhoneNumbers);

            contactInDb.PhoneNumbers = model.PhoneNumbers
                .Where(x => !string.IsNullOrWhiteSpace(x.Number))
                .Select(p => new PhoneNumber
                {
                    PhoneNumberId = p.PhoneNumberId > 0 ? p.PhoneNumberId : 0,
                    Number = p.Number,
                    ContactId = contactInDb.ContactId
                })
                .ToList();

            // =====================
            // SAVE
            // =====================
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task deleteContact(int? id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }

            await _context.SaveChangesAsync();
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.ContactId == id);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetHouseAddress(int contactId, int addressId)
        {
            if (!CurrentHouseId.HasValue)
                return BadRequest();

            // 1. Pobierz wszystkie kontakty z domu
            var contacts = await _context.Contacts
                .Include(x => x.Addresses)
                .Where(x => x.HouseId == CurrentHouseId.Value)
                .ToListAsync();

            // 2. ODZNACZ WSZYSTKIE adresy w całym domu
            foreach (var c in contacts)
            {
                foreach (var addr in c.Addresses)
                {
                    addr.IsHouseAddress = false;
                }
            }

            // 3. Znajdź wybrany adres
            var selected = contacts
                .SelectMany(x => x.Addresses)
                .FirstOrDefault(x => x.AddressId == addressId);

            if (selected != null)
            {
                selected.IsHouseAddress = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
