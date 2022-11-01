using StarLink_Contact.Models;

namespace StarLink_Contact.Services.Interfaces
{
    public interface IAddressBookService
    {
        public Task AddContactToCatagoryAsync(int catagoryId, int contactId);

        // add method: add to a list of categoriesIds
        public Task AddContactToCatagoriesAsync(IEnumerable<int> catagoryIds , int contactId);
        public Task<bool> IsContactInCatagory(int catagoryId, int contact);

        public Task<IEnumerable<Catagory>> GetAppUserCatagoriesAsync(string appUserId);

        // add method to remove form all categories
        public Task RemoveAllContactCatagoriesAsync(int contactId);
    }
}
