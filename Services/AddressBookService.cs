using Microsoft.EntityFrameworkCore;
using StarLink_Contact.Data;
using StarLink_Contact.Models;
using StarLink_Contact.Services.Interfaces;

namespace StarLink_Contact.Services
{
    public class AddressBookService : IAddressBookService
    {

         private readonly ApplicationDbContext _context;
        public AddressBookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddContactToCatagoriesAsync(IEnumerable<int> catagoryIds, int contactId)
        {
            try
            {
                Contact? contact = await _context.Contacts.FindAsync(contactId);

                foreach (int catagoryId in catagoryIds)
                {
                    Catagory? catagory = await _context.Catagories.FindAsync(catagoryId);
                    if (contact != null && catagory != null)
                    {

                        contact.Categories.Add(catagory);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        public async Task AddContactToCatagoryAsync(int catagoryId, int contactId)
        {
            try
            {
                // checks to see if contant is already in the category
                if (!await IsContactInCatagory(catagoryId,contactId))
                {
                    // if not... add the category to the contacts collections of the categories
                    Contact? contact = await _context.Contacts.FindAsync(contactId);
                    Catagory? catagory = await _context.Catagories.FindAsync(catagoryId);


                    if (contact != null && catagory != null)
                    {
                        catagory.Contacts.Add(contact);
                        await _context.SaveChangesAsync();
                    }

                }
             

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Catagory>> GetAppUserCatagoriesAsync(string appUserId)
        {
            List<Catagory> catagories = new List<Catagory>();
            try
            {
                catagories = await _context.Catagories
                    .Where(c=>c.AppUserId == appUserId)
                    .OrderBy(c => c.Name)
                    .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }

            return catagories;
        }

        public async Task<bool> IsContactInCatagory(int catagoryId, int contactId)
        {
            Contact? contact = await _context.Contacts.FindAsync(catagoryId);

            bool isInCatagory = await _context.Catagories
                .Include(c => c.Contacts)
                .Where(c => c.Id == catagoryId && c.Contacts
                .Contains(contact!))
                .AnyAsync();

            return isInCatagory;
        }

        public async Task RemoveAllContactCatagoriesAsync(int contactId)
        {
            try
            {
                Contact? contact = await _context.Contacts.Include(c => c.Categories).FirstOrDefaultAsync(c => c.Id == contactId);

                contact!.Categories.Clear();
                _context.Update(contact);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
