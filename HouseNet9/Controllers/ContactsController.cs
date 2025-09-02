using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Data.Data.HouseRentalData;
using HouseNet9.Data;

namespace HouseNet9.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Contacts
        public async Task<IActionResult> Index()
        {
            var contact = await _context.Contacts
                .Include(i => i.Addresses)
                .Include(i => i.EmailAddresses)
                .Include(i => i.PhoneNumbers)
                .ToArrayAsync();
            return View(contact);
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("ContactId,Name")] Contact contact)
        
       
        public async Task<IActionResult> Create(Contact contact)
        {
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
        public async Task<IActionResult> Edit(Contact model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Pobranie kontaktu z bazy wraz z kolekcjami
            var contactInDb = await _context.Contacts
                .Include(c => c.Addresses)
                .Include(c => c.PhoneNumbers)
                .Include(c => c.EmailAddresses)
                .FirstOrDefaultAsync(c => c.ContactId == model.ContactId);

            if (contactInDb == null)
            {
                return NotFound();
            }

            // Aktualizacja podstawowych danych
            contactInDb.Name = model.Name;

            // ===== Adresy =====
            var addressIds = model.Addresses?.Select(a => a.AddressId).ToList() ?? new List<int>();
            // Usuń te, które nie są już w modelu
            foreach (var address in contactInDb.Addresses.Where(a => !addressIds.Contains(a.AddressId)).ToList())
            {
                contactInDb.Addresses.Remove(address);
            }
            // Aktualizacja istniejących i dodanie nowych
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

            // Zapis do bazy danych
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("ContactId,Name")] Contact contact)
        //{
        //    if (id != contact.ContactId)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(contact);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ContactExists(contact.ContactId))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(contact);
        //}

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.ContactId == id);
        }
    }
}
