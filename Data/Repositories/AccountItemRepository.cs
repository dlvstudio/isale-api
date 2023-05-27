using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class AccountItemRepository: IAccountItemRepository {

    private readonly IConfiguration _config;

    public AccountItemRepository(IConfiguration config)
    {
        _config = config;
    }

    public AppDb AppDb
    {
        get
        {
            return new AppDb(_config.GetConnectionString("DefaultConnection"));
        }
    }

    public async Task<AccountItem> GetById(int itemId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM money_account_transaction 
                WHERE userId = @UserId
                    AND Id = @ItemId
                LIMIT 1
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<AccountItem>(query, new { UserId = userId, ItemId = itemId });
            return result.FirstOrDefault();
        }
    }

    public async Task<IEnumerable<AccountItem>> GetAccountItemsByAccount(string userId, int accountId, DateTime dateFrom, DateTime dateTo)
    {
        var dateFromOnlyDate = dateFrom.Date.ToString("yyyy-MM-dd");
        var dateToAddOne = dateTo.Date.AddDays(1).ToString("yyyy-MM-dd");
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM money_account_transaction 
                WHERE userId = @UserId
                    AND moneyAccountId = @AccountId
                    AND createdAt >= @DateFrom 
                    AND createdAt < @DateTo 
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<AccountItem>(query, new { UserId = userId, AccountId = accountId, DateFrom = dateFromOnlyDate, DateTo = dateToAddOne  });
            return result;
        }
    }

    public async Task<AccountItem> GetAccountItemByOrder(string userId, int orderId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM money_account_transaction 
                WHERE userId = @UserId
                    AND orderId = @OrderId
                LIMIT 1
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<AccountItem>(query, new { UserId = userId, OrderId = orderId });
            return result.FirstOrDefault();
        }
    }

    public async Task<AccountItem> GetAccountItemByTrade(string userId, int tradeId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM money_account_transaction 
                WHERE userId = @UserId
                    AND TradeId = @TradeId
                LIMIT 1
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<AccountItem>(query, new { UserId = userId, TradeId = tradeId });
            return result.FirstOrDefault();
        }
    }

    public async Task<bool> RemoveAccountItems(int accountId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM money_account_transaction 
                WHERE MoneyAccountId = @AccountId AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { AccountId = accountId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<bool> RemoveAccountItem(int itemId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM money_account_transaction 
                WHERE ID = @ItemId AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { ItemId = itemId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> SaveAccountItem(AccountItem accountItem)
    {
        if (accountItem == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (accountItem.Id > 0) {
                query = @"
                    UPDATE `money_account_transaction`
                    SET 
                        tradeId = @TradeId
                        ,orderId = @OrderId
                        ,moneyAccountId = @MoneyAccountId
                        ,note = @Note
                        ,value = @Value
                        ,transferFee = @TransferFee
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                accountItem.CreatedAt = DateTime.Now;
                query = @"INSERT INTO `money_account_transaction`
                    (   
                        `tradeId`,
                        `orderId`,
                        `moneyAccountId`,
                        `note`,
                        `value`,
                        `transferFee`,
                        `userId`
                        )
                    VALUES
                        (
                        @TradeId,
                        @OrderId,
                        @MoneyAccountId,
                        @Note,
                        @Value,
                        @TransferFee,
                        @UserId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, accountItem)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, accountItem);
            return postResult > 0 ? accountItem.Id : 0;
        }
    }
}