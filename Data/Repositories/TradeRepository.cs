using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class TradeRepository: ITradeRepository {

    private readonly IConfiguration _config;

    public TradeRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<Trade>> GetTrades(string userId, DateTime dateFrom, DateTime dateTo, int contactId, int productId, int staffId, int moneyAccountId, int orderId, int debtId, int receivedNoteId, int transferNoteId, int isReceived) {
        var dateFromOnlyDate = dateFrom.Date.ToString("yyyy-MM-dd");
        var dateToAddOne = dateTo.Date.AddDays(1).ToString("yyyy-MM-dd");
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM trade 
                WHERE UserId = @UserId
                    AND createdAt >= @DateFrom 
                    AND createdAt < @DateTo 
                    AND (@ContactId = 0 OR ContactId = @ContactId) 
                    AND (@ProductId = 0 OR ProductId = @ProductId) 
                    AND (@StaffId = 0 OR StaffId = @StaffId) 
                    AND (@OrderId = 0 OR OrderId = @OrderId) 
                    AND (@DebtId = 0 OR DebtId = @DebtId) 
                    AND (@IsReceived = -1 OR IsReceived = @IsReceived) 
                    AND (@ReceivedNoteId = 0 OR ReceivedNoteId = @ReceivedNoteId) 
                    AND (@MoneyAccountId = 0 OR MoneyAccountId = @MoneyAccountId) 
                    AND (@TransferNoteId = 0 OR TransferNoteId = @TransferNoteId) 
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Trade>(query, new { UserId = userId, DateFrom = dateFromOnlyDate, DateTo = dateToAddOne, ContactId = contactId, ProductId = productId, StaffId = staffId, MoneyAccountId = moneyAccountId, OrderId = orderId, DebtId = debtId, ReceivedNoteId = receivedNoteId, TransferNoteId = transferNoteId, IsReceived = isReceived });
            if (result != null && result.Any()) {
                var contactIds = result.Select(t => t.ContactId).Distinct();
                string contactsQuery = @"SELECT
                            *
                        FROM contact 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var contacts = await db.Connection.QueryAsync<Contact>(contactsQuery, new { UserId = userId, Ids = contactIds });
                foreach (var trade in result)
                {
                    if (trade.ContactId == 0) {
                        continue;
                    }
                    var contact = contacts.Where(c => c.Id == trade.ContactId).FirstOrDefault();
                    if (contact == null) {
                        continue;
                    }
                    trade.Contact = contact;
                }
            }
            if (result != null && result.Any()) {
                var productIds = result.Select(t => t.ProductId).Distinct();
                string productsQuery = @"SELECT
                            *
                        FROM product 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var products = await db.Connection.QueryAsync<Product>(productsQuery, new { UserId = userId, Ids = productIds });
                foreach (var trade in result)
                {
                    if (trade.ProductId == 0) {
                        continue;
                    }
                    var product = products.Where(c => c.Id == trade.ProductId).FirstOrDefault();
                    if (product == null) {
                        continue;
                    }
                    trade.Product = product;
                }
            }
            if (result != null && result.Any()) {
                var staffIds = result.Select(t => t.StaffId).Distinct();
                string staffsQuery = @"SELECT
                            *
                        FROM `staff` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var staffs = await db.Connection.QueryAsync<Staff>(staffsQuery, new { UserId = userId, Ids = staffIds });
                foreach (var trade in result)
                {
                    if (trade.StaffId == 0) {
                        continue;
                    }
                    var staff = staffs.Where(c => c.Id == trade.StaffId).FirstOrDefault();
                    if (staff == null) {
                        continue;
                    }
                    trade.Staff = staff;
                }
                var orderIds = result.Select(t => t.OrderId).Distinct();
                string ordersQuery = @"SELECT
                            *
                        FROM `order` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var orders = await db.Connection.QueryAsync<Order>(ordersQuery, new { UserId = userId, Ids = orderIds });
                foreach (var trade in result)
                {
                    if (trade.OrderId == 0) {
                        continue;
                    }
                    var item = orders.Where(c => c.Id == trade.OrderId).FirstOrDefault();
                    if (item == null) {
                        continue;
                    }
                    trade.Order = item;
                }
                var accountIds = result.Select(t => t.MoneyAccountId).Distinct();
                string accountsQuery = @"SELECT
                            *
                        FROM `money_account` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var accounts = await db.Connection.QueryAsync<Account>(accountsQuery, new { UserId = userId, Ids = accountIds });
                foreach (var trade in result)
                {
                    if (trade.MoneyAccountId == 0) {
                        continue;
                    }
                    var item = accounts.Where(c => c.Id == trade.MoneyAccountId).FirstOrDefault();
                    if (item == null) {
                        continue;
                    }
                    trade.MoneyAccount = item;
                }
                var debtIds = result.Select(t => t.DebtId).Distinct();
                string debtsQuery = @"SELECT
                            *
                        FROM `debt` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var debts = await db.Connection.QueryAsync<Debt>(debtsQuery, new { UserId = userId, Ids = debtIds });
                foreach (var trade in result)
                {
                    if (trade.DebtId == 0) {
                        continue;
                    }
                    var item = debts.Where(c => c.Id == trade.DebtId).FirstOrDefault();
                    if (item == null) {
                        continue;
                    }
                    trade.Debt = item;
                }

                var receivedNoteIds = result.Select(t => t.ReceivedNoteId).Distinct();
                string receivedNotesQuery = @"SELECT
                            *
                        FROM `received_note` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var receivedNotes = await db.Connection.QueryAsync<ReceivedNote>(receivedNotesQuery, new { UserId = userId, Ids = receivedNoteIds });
                foreach (var trade in result)
                {
                    if (trade.ReceivedNoteId == 0) {
                        continue;
                    }
                    var item = receivedNotes.Where(c => c.Id == trade.ReceivedNoteId).FirstOrDefault();
                    if (item == null) {
                        continue;
                    }
                    trade.ReceivedNote = item;
                }
            }
            return result;
        }
    }

    public async Task<Trade> GetById(int tradeId, string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM trade 
                WHERE Id = @Id AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var results = await db.Connection.QueryAsync<Trade>(query, new { Id = tradeId, UserId = userId });
            var result = results.FirstOrDefault();
            if (result != null && result.ContactId != 0) {
                string contactsQuery = @"SELECT
                            *
                        FROM contact 
                        WHERE UserId = @UserId
                            AND ID = @ContactId
                        LIMIT 1
                    ";
                var contacts = await db.Connection.QueryAsync<Contact>(contactsQuery, new { UserId = userId, ContactId = result.ContactId });
                var contact = contacts.FirstOrDefault();
                result.Contact = contact;
            }
            if (result != null && result.ProductId != 0) {
                string productsQuery = @"SELECT
                            *
                        FROM product 
                        WHERE UserId = @UserId
                            AND ID = @ProductId
                    ";
                var products = await db.Connection.QueryAsync<Product>(productsQuery, new { UserId = userId, ProductId = result.ProductId });
                var product = products.FirstOrDefault();
                result.Product = product;
            }
            if (result != null && result.StaffId != 0) {
                string productsQuery = @"SELECT
                            *
                        FROM `staff` 
                        WHERE UserId = @UserId
                            AND ID = @StaffId
                    ";
                var staffs = await db.Connection.QueryAsync<Staff>(productsQuery, new { UserId = userId, StaffId = result.StaffId });
                var staff = staffs.FirstOrDefault();
                result.Staff = staff;
            }
            if (result != null && result.MoneyAccountId != 0) {
                string productsQuery = @"SELECT
                            *
                        FROM `money_account` 
                        WHERE UserId = @UserId
                            AND ID = @MoneyAccountId
                    ";
                var items = await db.Connection.QueryAsync<Account>(productsQuery, new { UserId = userId, MoneyAccountId = result.MoneyAccountId });
                var account = items.FirstOrDefault();
                result.MoneyAccount = account;
            }
            if (result != null && result.OrderId != 0) {
                string productsQuery = @"SELECT
                            *
                        FROM `order` 
                        WHERE UserId = @UserId
                            AND ID = @OrderId
                    ";
                var items = await db.Connection.QueryAsync<Order>(productsQuery, new { UserId = userId, OrderId = result.OrderId });
                var item = items.FirstOrDefault();
                result.Order = item;
            }
            if (result != null && result.DebtId != 0) {
                string productsQuery = @"SELECT
                            *
                        FROM `debt` 
                        WHERE UserId = @UserId
                            AND ID = @DebtId
                    ";
                var items = await db.Connection.QueryAsync<Debt>(productsQuery, new { UserId = userId, DebtId = result.DebtId });
                var item = items.FirstOrDefault();
                result.Debt = item;
            }
            if (result != null && result.ReceivedNoteId != 0) {
                string productsQuery = @"SELECT
                            *
                        FROM `received_note` 
                        WHERE UserId = @UserId
                            AND ID = @ReceivedNoteId
                    ";
                var items = await db.Connection.QueryAsync<ReceivedNote>(productsQuery, new { UserId = userId, ReceivedNoteId = result.ReceivedNoteId });
                var item = items.FirstOrDefault();
                result.ReceivedNote = item;
            }
            return result;
        }
    }

    public async Task<bool> Remove(int tradeId, string userId) {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM trade 
                WHERE Id = @Id AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Id = tradeId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> SaveTrade(Trade trade) {
        if (trade == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (trade.Id > 0) {
                trade.ModifiedAt = DateTime.Now;
                query = @"
                    UPDATE `trade`
                    SET 
                        avatarUrl = @AvatarUrl
                        ,imageUrlsJson = @ImageUrlsJson
                        ,contactId = @ContactId
                        ,productId = @ProductId
                        ,staffId = @StaffId
                        ,orderId = @OrderId
                        ,receivedNoteId = @ReceivedNoteId
                        ,debtId = @DebtId
                        ,moneyAccountId = @MoneyAccountId
                        ,productCount = @ProductCount
                        ,isReceived = @IsReceived
                        ,value = @Value
                        ,fee = @Fee
                        ,note = @Note
                        ,modifiedAt = @ModifiedAt
                        ,createdAt = @CreatedAt
                        ,isPurchase = @IsPurchase
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                trade.ModifiedAt = DateTime.Now;
                query = @"INSERT INTO `trade`
                    (   
                        `avatarUrl`,
                        `imageUrlsJson`,
                        `contactId`,
                        `productId`,
                        `staffId`,
                        `orderId`,
                        `receivedNoteId`,
                        `debtId`,
                        `moneyAccountId`,
                        `isReceived`,
                        `value`,
                        `fee`,
                        `note`,
                        `isPurchase`,
                        `productCount`,
                        `createdAt`,
                        `modifiedAt`,
                        `userId`
                        )
                    VALUES
                        (
                        @AvatarUrl,
                        @ImageUrlsJson,
                        @ContactId,
                        @ProductId,
                        @StaffId,
                        @OrderId,
                        @ReceivedNoteId,
                        @DebtId,
                        @MoneyAccountId,
                        @IsReceived,
                        @Value,
                        @Fee,
                        @Note,
                        @IsPurchase,
                        @ProductCount,
                        @CreatedAt,
                        @ModifiedAt,
                        @UserId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, trade)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, trade);
            return postResult > 0 ? trade.Id : 0;
        }
    }

    public AppDb AppDb
    {
        get
        {
            return new AppDb(_config.GetConnectionString("DefaultConnection"));
        }
    }
}