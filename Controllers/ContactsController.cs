using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarLink_Contact.Data;
using StarLink_Contact.Enums;
using StarLink_Contact.Models;
using StarLink_Contact.Services.Interfaces;

namespace StarLink_Contact.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService  _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailSender;

        public ContactsController(ApplicationDbContext context,
                              UserManager<AppUser> userManager,
                              IImageService imageService, 
                              IAddressBookService addressBookService,
                              IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
            _emailSender = emailSender;
        }

        // GET: Contacts


        [Authorize]
        public async Task<IActionResult> Index(int? catagoryId, string? swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;
            string userId = _userManager.GetUserId(User);


            List<Contact> contacts = new List<Contact>();
            
           List<Catagory> Catagories = await _context.Catagories.Where(c => c.AppUserId == userId).ToListAsync();


            if (catagoryId == null)
            {
                contacts = await _context.Contacts
                    .Where(c => c.AppUserId == userId)
                    .Include(c => c.AppUser)
                    .Include(c => c.Categories)
                    .ToListAsync();
            }
            else
            {
                contacts = (await _context.Catagories
                    .Include(c => c.Contacts)
                    .FirstOrDefaultAsync(c => c.AppUserId == userId && c.Id == catagoryId))!.Contacts.ToList();
            }




            ViewData["CategoryId"] = new SelectList(Catagories, "Id", "Name", catagoryId);

            return View(contacts);
        }

        //index view
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchContacts(string? searchString)
        {
            string appUserId = _userManager.GetUserId(User);

            List<Contact> contacts = new List<Contact>();

            AppUser? appUser = await _context.Users
                                      .Include(c => c.Contacts)
                                      .ThenInclude(c => c.Categories)
                                      .FirstOrDefaultAsync(u => u.Id == appUserId);

            if (string.IsNullOrEmpty(searchString))
            {
                contacts = appUser!.Contacts
                                  .OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                  .ToList();
            }
            else
            {
                contacts = appUser!.Contacts
                                  .Where(c => c.FullName!
                                  .ToLower()
                                  .Contains(searchString.ToLower()))
                                  .OrderBy(c => c.LastName)
                                  .ThenBy(c => c.FirstName)
                                  .ToList();
            }

            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name");
            return View(nameof(Index), contacts);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EmailContact(int? id)
        {
            string appUserId = _userManager.GetUserId(User);
            Contact? contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);


            if (contact == null)
            {
                return NotFound();
            }

            EmailData emailData = new EmailData()
            {
                EmailAddress = contact.Email,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
            };

            EmailContactViewModel viewmodel = new EmailContactViewModel()
           { 
                Contact = contact,
                EmailData = emailData
            };



            return View(viewmodel);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailContact(EmailContactViewModel viewmodel)
        {
            if (ModelState.IsValid)
            {
                string? swalMessage = string.Empty;
                try
                {
                    await _emailSender.SendEmailAsync(viewmodel.EmailData!.EmailAddress, viewmodel.EmailData!.EmailSubject, viewmodel.EmailData.EmailBody);
                    swalMessage = "Success: Email sent!";
                    return RedirectToAction("Index", "Contacts", new { swalMessage });
                }
                catch (Exception)
                {
                    swalMessage = " Error: email Send Failed!";
                    return RedirectToAction("Index", "Contacts", new { swalMessage });
                    throw;
                }
              
                   
                
            }
            return View(viewmodel);
        }


        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {

            ViewData["StateList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());


            //TODO: CATEGOREIES DROP DOWN 
            string userId = _userManager.GetUserId(User);
            List<Catagory> catagories = await _context.Catagories.Where(c => c.AppUserId == userId).ToListAsync();
            ViewData["CategoryList"] = new MultiSelectList(catagories,"Id","Name");
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageData,ImageType,ImageFile")] Contact contact, List<int> categoryList)
        {
            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                contact.AppUserId = _userManager.GetUserId(User);
                contact.Created = DateTime.UtcNow;

                if(contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                }

                //check whether a file/image has been selected 
                //if Image is NOT null set the ImageData property - convert the file to byte[]
                //if Image is NOT null set the ImageType property - Use the file extension as the value 

                if(contact.ImageFile != null)
                {
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    contact.ImageType = contact.ImageFile.ContentType;
                }


                _context.Add(contact);
                await _context.SaveChangesAsync();

                //TODO: use the list od categories IDS to...
                // 1. Find the associated Category 
                //2. Add the category to the collection of categories for the current Contact 
                foreach(int categoryId in categoryList)
                {
                    await _addressBookService.AddContactToCatagoryAsync(categoryId, contact.Id);
                }




                return RedirectToAction(nameof(Index));
            }
            ViewData["StateList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            string userId = _userManager.GetUserId(User);
            List<Catagory> catagories = await _context.Catagories.Where(c => c.AppUserId == userId).ToListAsync();
            ViewData["CategoryList"] = new MultiSelectList(catagories, "Id", "Name");
            
            return View(contact);
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }




            string appUserId = _userManager.GetUserId(User);

            Contact? contact = await _context.Contacts
                .Include(c => c.Categories)
                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);




            if (contact == null)
            {
                return NotFound();
            }
            ViewData["StateList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            // TODO: Add cateregory list 





            // load data for the custom category 

            List<Catagory> catagories = (await _addressBookService.GetAppUserCatagoriesAsync(appUserId)).ToList();
            List<int> catagoryId = contact.Categories.Select(c => c.Id).ToList();


            ViewData["CategoryList"] = new MultiSelectList(catagories, "Id", "Name", catagoryId);
            
            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created,ImageFile,ImageData,ImageType")] Contact contact, List<int> categoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.Created = DateTime.SpecifyKind(contact.Created,DateTimeKind.Utc);
                    if (contact.BirthDate != null)
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }

                    //check whether a file/image has been selected 
                    //if Image is NOT null set the ImageData property - convert the file to byte[]
                    //if Image is NOT null set the ImageType property - Use the file extension as the value 

                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }

                    _context.Update(contact);
                    await _context.SaveChangesAsync();


                    //TODO: Add Categories code
                    //Remove current categories 

                    await _addressBookService.RemoveAllContactCatagoriesAsync(contact.Id);

                    // Add selected catagories to the contact
                    await _addressBookService.AddContactToCatagoriesAsync(categoryList, contact.Id);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
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

            List<Catagory> catagories = (await _addressBookService.GetAppUserCatagoriesAsync(contact.AppUserId!)).ToList();
            List<int> catagoryId = contact.Categories.Select(c => c.Id).ToList();


            ViewData["CategoryList"] = new MultiSelectList(catagories, "Id", "Name", catagoryId);

            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contacts == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            if (_context.Contacts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contacts'  is null.");
            }
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
          return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
