using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;
    private readonly IAccountItemRepository _itemRepository;

    public AccountService(
        IAccountRepository repository,
        IAccountItemRepository itemRepository
    ) {
        _repository = repository;
        _itemRepository = itemRepository;
    }

    public async Task<IEnumerable<Account>> GetAccounts(string userId)
    {
        return await _repository.GetAccounts(userId);
    }

    public async Task<Account> GetAccount(int id, string userId) {
        var post = await _repository.GetById(id, userId);
        return post;
    }

    public async Task<AccountItem> GetAccountItem(int id, string userId) {
        var accountItem = await _itemRepository.GetById(id, userId);
        var account = await GetAccount(accountItem.MoneyAccountId, userId);
        accountItem.MoneyAccount = account;
        return accountItem;
    }

    public async Task<bool> RemoveAccount(int id, string userId) {
        var post = await _repository.Remove(id, userId);
        return post;
    }

    public async Task<int> SaveAccount(Account account) {
        return await _repository.SaveAccount(account);
    }

    public async Task<IEnumerable<AccountItem>> GetAccountItemsByAccount(string userId, int accountId, DateTime? dateFrom, DateTime? dateTo)
    {
        var items = await _itemRepository.GetAccountItemsByAccount(userId, accountId, 
            dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6), 
            dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3));
        var account = await GetAccount(accountId, userId);
        foreach (var item in items)
        {
            item.MoneyAccount = account;
        }
        return items;
    }

    public async Task<AccountItem> GetAccountItemByTrade(string userId, int accountId)
    {
        var accountItem = await _itemRepository.GetAccountItemByTrade(userId, accountId);
        if (accountItem == null) {
            return null;
        }
        var account = await GetAccount(accountItem.MoneyAccountId, userId);
        accountItem.MoneyAccount = account;
        return accountItem;
    }

    public async Task<AccountItem> GetAccountItemByOrder(string userId, int accountId)
    {
        var accountItem  = await _itemRepository.GetAccountItemByOrder(userId, accountId);
        if (accountItem == null) {
            return null;
        }
        var account = await GetAccount(accountItem.MoneyAccountId, userId);
        accountItem.MoneyAccount = account;
        return accountItem;
    }

    public async Task<bool> RemoveAccountItems(int accountId, string userId)
    {
        return await _itemRepository.RemoveAccountItems(accountId, userId);
    }

    public async Task<bool> RemoveAccountItem(int itemId, string userId)
    {
        return await _itemRepository.RemoveAccountItem(itemId, userId);
    }

    public async Task<int> SaveAccountItem(AccountItem accountItem)
    {
        return await _itemRepository.SaveAccountItem(accountItem);
    }

    public async Task<Account> GetDefault(string userId)
    {
        var post = await _repository.GetDefault(userId);
        return post;
    }

    public async Task<Account> GetStoreDefault(string userId, int storeId)
    {
        var post = await _repository.GetStoreDefault(userId, storeId);
        return post;
    }
}