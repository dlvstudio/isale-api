using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class DebtRepository: IDebtRepository {

    private readonly IConfiguration _config;

    public DebtRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<Debt>> GetDebts(string userId, DateTime dateFrom, DateTime dateTo, int contactId, int productId, int debtType, int orderId, int receivedNoteId, int staffId, int storeId) {
        var dateFromOnlyDate = dateFrom.Date.ToString("yyyy-MM-dd");
        var dateToAddOne = dateTo.Date.AddDays(1).ToString("yyyy-MM-dd");
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM debt 
                WHERE UserId = @UserId
                    AND createdAt >= @DateFrom 
                    AND createdAt < @DateTo 
                    AND (@StaffId = 0 OR StaffId = @StaffId) 
                    AND (@ContactId = 0 OR ContactId = @ContactId) 
                    AND (@ProductId = 0 OR ProductId = @ProductId) 
                    AND (@StoreId = 0 OR StoreId = @StoreId) 
                    AND (@DebtType = 0 OR DebtType = @DebtType) 
                    AND (@ReceivedNoteId = 0 OR ReceivedNoteId = @ReceivedNoteId) 
                    AND (@OrderId = 0 OR OrderId = @OrderId) 
                ORDER BY createdAt DESC
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Debt>(query, new { UserId = userId, DateFrom = dateFromOnlyDate, DateTo = dateToAddOne, ContactId = contactId, ProductId = productId, DebtType = debtType, OrderId = orderId, ReceivedNoteId = receivedNoteId, StaffId = staffId, StoreId = storeId });
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
                var staffIds = result.Select(t => t.StaffId).Distinct();
                string staffsQuery = @"SELECT
                            *
                        FROM `staff` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var staffs = await db.Connection.QueryAsync<Staff>(staffsQuery, new { UserId = userId, Ids = staffIds });
                foreach (var debt in result)
                {
                    if (debt.StaffId == 0) {
                        continue;
                    }
                    var staff = staffs.Where(c => c.Id == debt.StaffId).FirstOrDefault();
                    if (staff == null) {
                        continue;
                    }
                    debt.Staff = staff;
                }
            }
            if (result != null && result.Any()) {
                var productIds = result.Select(t => t.OrderId).Distinct();
                string ordersQuery = @"SELECT
                            *
                        FROM `order` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var orders = await db.Connection.QueryAsync<Order>(ordersQuery, new { UserId = userId, Ids = productIds });
                foreach (var trade in result)
                {
                    if (trade.OrderId == 0) {
                        continue;
                    }
                    var product = orders.Where(c => c.Id == trade.OrderId).FirstOrDefault();
                    if (product == null) {
                        continue;
                    }
                    trade.Order = product;
                }
            }
            if (result != null && result.Any()) {
                var productIds = result.Select(t => t.ReceivedNoteId).Distinct();
                string ordersQuery = @"SELECT
                            *
                        FROM `received_note` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var orders = await db.Connection.QueryAsync<ReceivedNote>(ordersQuery, new { UserId = userId, Ids = productIds });
                foreach (var trade in result)
                {
                    if (trade.ReceivedNoteId == 0) {
                        continue;
                    }
                    var product = orders.Where(c => c.Id == trade.ReceivedNoteId).FirstOrDefault();
                    if (product == null) {
                        continue;
                    }
                    trade.ReceivedNote = product;
                }
            }
            return result;
        }
    }

    public async Task<Debt> GetById(int debtId, string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM debt 
                WHERE Id = @Id AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var results = await db.Connection.QueryAsync<Debt>(query, new { Id = debtId, UserId = userId });
            var result = results.FirstOrDefault();
            if (result != null && result.StaffId != 0) {
                string staffsQuery = @"SELECT
                            *
                        FROM `staff` 
                        WHERE UserId = @UserId
                            AND ID = @StaffId
                    ";
                var staffs = await db.Connection.QueryAsync<Staff>(staffsQuery, new { UserId = userId, StaffId = result.StaffId });
                var staff = staffs.FirstOrDefault();
                result.Staff = staff;
            }
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
            if (result != null && result.OrderId != 0) {
                string productsQuery = @"SELECT
                            *
                        FROM `order` 
                        WHERE UserId = @UserId
                            AND ID = @OrderId
                    ";
                var products = await db.Connection.QueryAsync<Order>(productsQuery, new { UserId = userId, OrderId = result.OrderId });
                var product = products.FirstOrDefault();
                result.Order = product;
            }
            if (result != null && result.ReceivedNoteId != 0) {
                string productsQuery = @"SELECT
                            *
                        FROM `received_note` 
                        WHERE UserId = @UserId
                            AND ID = @ReceivedNoteId
                    ";
                var products = await db.Connection.QueryAsync<ReceivedNote>(productsQuery, new { UserId = userId, ReceivedNoteId = result.ReceivedNoteId });
                var product = products.FirstOrDefault();
                result.ReceivedNote = product;
            }
            return result;
        }
    }

    public async Task<bool> Remove(int tradeId, string userId) {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM debt 
                WHERE Id = @Id AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Id = tradeId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> SaveDebt(Debt debt) {
        if (debt == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (debt.Id > 0) {
                debt.ModifiedAt = DateTime.Now;
                string updateCreatedAt = string.Empty;
                if (debt.CreatedAt.HasValue) {
                    updateCreatedAt = ",createdAt = @CreatedAt";
                }
                query = @"
                    UPDATE `debt`
                    SET 
                        contactId = @ContactId
                        ,productId = @ProductId
                        ,orderId = @OrderId
                        ,receivedNoteId = @ReceivedNoteId
                        ,debtType = @DebtType
                        ,value = @Value
                        ,valuePaid = @ValuePaid
                        ,countPaid = @CountPaid
                        ,note = @Note
                        ,modifiedAt = @ModifiedAt
                        ,isPurchase = @IsPurchase
                        ,productCount = @ProductCount
                        ,interestRate = @InterestRate
                        ,maturityDate = @MaturityDate
                        ,isPaid = @IsPaid
                        ,staffId = @StaffId
                        ,storeId = @StoreId
                        " + updateCreatedAt + @"
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                debt.CreatedAt = debt.CreatedAt.HasValue ? debt.CreatedAt.Value : DateTime.Now;
                query = @"INSERT INTO `debt`
                    (   
                        `contactId`,
                        `productId`,
                        `orderId`,
                        `receivedNoteId`,
                        `debtType`,
                        `value`,
                        `valuePaid`,
                        `countPaid`,
                        `note`,
                        `isPurchase`,
                        `productCount`,
                        `interestRate`,
                        `createdAt`,
                        `maturityDate`,
                        `isPaid`,
                        `userId`,
                        `storeId`,
                        `staffId`
                        )
                    VALUES
                        (
                        @ContactId,
                        @ProductId,
                        @OrderId,
                        @ReceivedNoteId,
                        @DebtType,
                        @Value,
                        @ValuePaid,
                        @CountPaid,
                        @Note,
                        @IsPurchase,
                        @ProductCount,
                        @InterestRate,
                        @CreatedAt,
                        @MaturityDate,
                        @IsPaid,
                        @UserId,
                        @StoreId,
                        @StaffId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, debt)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, debt);
            return postResult > 0 ? debt.Id : 0;
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