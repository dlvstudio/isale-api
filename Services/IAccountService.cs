using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAccountService
{
    Task<IEnumerable<Account>> GetAccounts(string userId);

    Task<Account> GetAccount(int id, string userId);

    Task<AccountItem> GetAccountItem(int id, string userId);

    Task<Account> GetDefault(string userId);

    Task<Account> GetStoreDefault(string userId, int storeId);

    Task<bool> RemoveAccount(int id, string userId);

    Task<int> SaveAccount(Account account);

    Task<IEnumerable<AccountItem>> GetAccountItemsByAccount(string userId, int accountId, DateTime? dateFrom, DateTime? dateTo);

    Task<AccountItem> GetAccountItemByTrade(string userId, int tradeId);

    Task<AccountItem> GetAccountItemByOrder(string userId, int orderId);

    Task<bool> RemoveAccountItems(int accountId, string userId);

    Task<bool> RemoveAccountItem(int itemId, string userId);

    Task<int> SaveAccountItem(AccountItem accountItem);

}