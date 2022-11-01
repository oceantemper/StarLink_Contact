using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StarLink_Contact.Data;
using StarLink_Contact.Models;

namespace StarLink_Contact.Controllers
{
    public class CatagoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender;

        public CatagoriesController(ApplicationDbContext context, UserManager<AppUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: Catagories
        [Authorize]
        public async Task<IActionResult> Index(string? swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;

            string userId = _userManager.GetUserId(User);
            List<Catagory> catagories = await _context.Catagories.Where(c => c.AppUserId == userId).Include(c => c.AppUser).ToListAsync();
            return View(catagories);
        }

        [Authorize]
        [HttpGet]
        public async Task <IActionResult> EmailCatagory(int? id)
        {
            //allow the current user to get htis category by id 
            //only if it is their category
            string appUserId = _userManager.GetUserId(User);
            Catagory? catagory = await _context.Catagories.Include(c=>c.Contacts).FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);

            if (catagory == null)
            {
                NotFound();
            }
          
            List<string> emails = catagory!.Contacts.Select(c=>c.Email).ToList()!;


            EmailData emailData = new EmailData()
            {
               GroupName = catagory.Name,
               EmailAddress = string.Join(";",emails),
               EmailSubject = $"Group Message: {catagory.Name}"
            };

        
            return View(emailData);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailCatagory(EmailData emailData)
        {

            if (ModelState.IsValid)
            {
                string swalMessage = string.Empty;
                try
                {
                    await _emailSender.SendEmailAsync(emailData!.EmailAddress,emailData!.EmailSubject, emailData.EmailBody);
                    swalMessage = "Success: Email sent!";
                    return RedirectToAction("Index", "Catagories", new { swalMessage });
                }
                catch (Exception)
                {
                    swalMessage = " Error: email Send Failed!";
                    return RedirectToAction("Index", "Catagories", new { swalMessage });
                    throw;
                }



            }
            return View(emailData);

        }

        // GET: Catagories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Catagories == null)
            {
                return NotFound();
            }

            var catagory = await _context.Catagories
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (catagory == null)
            {
                return NotFound();
            }

            return View(catagory);
        }

        // GET: Catagories/Create

        public IActionResult Create()
        {
            
            return View();
        }

        // POST: Catagories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Catagory catagory)
        {

            ModelState.Remove("AppUserId");

            if (ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User);
                catagory.AppUserId = userId;

                _context.Add(catagory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
           // ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", catagory.AppUserId);
            return View(catagory);
        }

        // GET: Catagories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Catagories == null)
            {
                return NotFound();
            }

            Catagory? catagory = await _context.Catagories.FindAsync(id);
            if (catagory == null)
            {
                return NotFound();
            }

           
            return View(catagory);
        }

        // POST: Catagories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,Name")] Catagory catagory)
        {
            if (id != catagory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(catagory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CatagoryExists(catagory.Id))
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
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", catagory.AppUserId);
            return View(catagory);
        }

        // GET: Catagories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Catagories == null)
            {
                return NotFound();
            }

            var catagory = await _context.Catagories
                .Include(c => c.AppUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (catagory == null)
            {
                return NotFound();
            }

            return View(catagory);
        }

        // POST: Catagories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Catagories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var catagory = await _context.Catagories.FindAsync(id);
            if (catagory != null)
            {
                _context.Catagories.Remove(catagory);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CatagoryExists(int id)
        {
          return _context.Catagories.Any(e => e.Id == id);
        }
    }
}
