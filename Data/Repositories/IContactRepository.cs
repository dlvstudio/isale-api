using System.Collections.Generic;
using System.Threading.Tasks;

public interface IContactRepository {

    Task<IEnumerable<Contact>> GetContacts(string userId);

    Task<Contact> GetById(int contactId, string userId);
    
    Task<Contact> GetByCode(string contactCode, string userId);

    Task<bool> Remove(int contactId, string userId);

    Task<int> SaveContact(Contact contact);
}