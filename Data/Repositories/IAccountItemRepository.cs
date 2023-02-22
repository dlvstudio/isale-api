using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IAccountItemRepository {
    Task<AccountItem> GetById(int itemId, string userId);

    Task<IEnumerable<AccountItem>> GetAccountItemsByAccount(string userId, int accountId, DateTime dateFrom, DateTime dateTo);

    Task<AccountItem> GetAccountItemByTrade(string userId, int accountId);

    Task<AccountItem> GetAccountItemByOrder(string userId, int accountId);

    Task<bool> RemoveAccountItems(int id, string userId);

    Task<bool> RemoveAccountItem(int itemId, string userId);

    Task<int> SaveAccountItem(AccountItem accountItem);
}