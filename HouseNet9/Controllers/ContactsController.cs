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
        :base(context, loggerFactory)
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

        // GET: Contacts/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var contact = await _context.Contacts
        //        .FirstOrDefaultAsync(m => m.ContactId == id);
        //    if (contact == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(contact);
        //}

        // GET: Contacts/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Contacts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contact contact)
        {
            var houseId = CurrentHouseId;
            if (!houseId.HasValue)
            {
                return BadRequest("Nie wybrano domu.");
            }

            // Wymuszamy poprawny HouseId
            contact.HouseId = CurrentHouseId;

            if (ModelState.IsValid)
            {
                _context.Add(contact);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(contact);
        }

        //GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Where(w => w.ContactId == id)
                .Include(i => i.Addresses)
                .Include(i => i.EmailAddresses)
                .Include(i => i.PhoneNumbers)
                .FirstOrDefaultAsync();
            if (contact == null)
            {
                return NotFound();
            }
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
            else
            {
                if (!ModelState.IsValid)
                    return View(model);

                // Pobranie kontaktu z bazy wraz z kolekcjami
                var contactInDb = await _context.Contacts
                    .Include(c => c.Addresses)
                    .Include(c => c.PhoneNumbers)
                    .Include(c => c.EmailAddresses)
                    .FirstOrDefaultAsync(c => c.ContactId == model.ContactId);

                if (contactInDb == null)
                    return NotFound();

                // ===== Aktualizacja podstawowych danych =====
                contactInDb.Name = model.Name;

                // ===== Ustawienie domu =====
                var houseId = CurrentHouseId;
                if (houseId.HasValue)
                    contactInDb.HouseId = houseId.Value;

                // ===== Adresy =====
                var addressIds = model.Addresses?.Select(a => a.AddressId).ToList() ?? new List<int>();
                foreach (var address in contactInDb.Addresses.Where(a => !addressIds.Contains(a.AddressId)).ToList())
                {
                    contactInDb.Addresses.Remove(address);
                    _context.Addresses.Remove(address); // usuń fizycznie z bazy
                }

                if (model.Addresses != null)
                {
                    foreach (var address in model.Addresses)
                    {
                        var existing = contactInDb.Addresses.FirstOrDefault(a => a.AddressId == address.AddressId);
                        if (existing != null)
                        {
                            existing.Street = address.Street;
                            existing.PostalCode = address.PostalCode;
                            existing.City = address.City;
                            existing.Country = address.Country;
                        }
                        else
                        {
                            contactInDb.Addresses.Add(new Address
                            {
                                Street = address.Street,
                                PostalCode = address.PostalCode,
                                City = address.City,
                                Country = address.Country
                            });
                        }
                    }
                }

                // ===== E-maile =====
                var emailIds = model.EmailAddresses?.Select(e => e.EmailAddressId).ToList() ?? new List<int>();
                foreach (var email in contactInDb.EmailAddresses.Where(e => !emailIds.Contains(e.EmailAddressId)).ToList())
                {
                    contactInDb.EmailAddresses.Remove(email);
                    _context.EmailAddresses.Remove(email);
                }

                if (model.EmailAddresses != null)
                {
                    foreach (var email in model.EmailAddresses)
                    {
                        var existing = contactInDb.EmailAddresses.FirstOrDefault(e => e.EmailAddressId == email.EmailAddressId);
                        if (existing != null)
                        {
                            existing.Email = email.Email;
                        }
                        else
                        {
                            contactInDb.EmailAddresses.Add(new EmailAddress
                            {
                                Email = email.Email
                            });
                        }
                    }
                }

                // ===== Telefony =====
                var phoneIds = model.PhoneNumbers?.Select(p => p.PhoneNumberId).ToList() ?? new List<int>();
                foreach (var phone in contactInDb.PhoneNumbers.Where(p => !phoneIds.Contains(p.PhoneNumberId)).ToList())
                {
                    contactInDb.PhoneNumbers.Remove(phone);
                    _context.PhoneNumbers.Remove(phone);
                }

                if (model.PhoneNumbers != null)
                {
                    foreach (var phone in model.PhoneNumbers)
                    {
                        var existing = contactInDb.PhoneNumbers.FirstOrDefault(p => p.PhoneNumberId == phone.PhoneNumberId);
                        if (existing != null)
                        {
                            existing.Number = phone.Number;
                        }
                        else
                        {
                            contactInDb.PhoneNumbers.Add(new PhoneNumber
                            {
                                Number = phone.Number
                            });
                        }
                    }
                }

                // ===== Zapis do bazy =====
                await _context.SaveChangesAsync();

                // Przekierowanie do strony domu
                //return RedirectToAction("Details", "Houses", new { id = houseId });
                return RedirectToAction(nameof(Index));
            }
        }


        private async Task deleteContact(int? id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }

            //bez cascade
            ////var contact = await _context.Contacts
            ////                    .Include(c => c.Addresses)
            ////                    .Include(c => c.PhoneNumbers)
            ////                    .Include(c => c.EmailAddresses)
            ////                    .FirstOrDefaultAsync(c => c.ContactId == id);

            ////if (contact != null)
            ////{
            ////    _context.Contacts.Remove(contact);
            ////    await _context.SaveChangesAsync();
            ////}



            await _context.SaveChangesAsync();
        }


        //Contacts/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    var contact = await _context.Contacts.FindAsync(id);
        //    if (contact != null)
        //    {
        //        _context.Contacts.Remove(contact);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.ContactId == id);
        }
    }
}
