using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAccountRepository {

    Task<IEnumerable<Account>> GetAccounts(string userId);

    Task<Account> GetById(int accountId, string userId);

    Task<Account> GetDefault(string userId);

    Task<Account> GetStoreDefault(string userId, int storeId);

    Task<bool> Remove(int accountId, string userId);

    Task<int> SaveAccount(Account account);
}