using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class ReceivedNoteRepository: IReceivedNoteRepository {

    private readonly IConfiguration _config;

    public ReceivedNoteRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<ReceivedNote>> List(string userId, DateTime dateFrom, DateTime dateTo, int contactId, int staffId, int storeId) {
        var dateFromOnlyDate = dateFrom.Date.ToString("yyyy-MM-dd");
        var dateToAddOne = dateTo.Date.AddDays(1).ToString("yyyy-MM-dd");
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `received_note` 
                WHERE UserId = @UserId
                    AND (@ContactId = 0 OR ContactId = @ContactId) 
                    AND (@StaffId = 0 OR StaffId = @StaffId) 
                    AND (@StoreId = 0 OR StoreId = @StoreId) 
                    AND createdAt >= @DateFrom 
                    AND createdAt < @DateTo 
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<ReceivedNote>(query, new { UserId = userId, DateFrom = dateFromOnlyDate, DateTo = dateToAddOne, ContactId = contactId, StaffId = staffId, StoreId = storeId });
            if (result != null && result.Any()) {
                var contactIds = result.Select(t => t.ContactId).Distinct();
                string contactsQuery = @"SELECT
                            *
                        FROM contact 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var contacts = await db.Connection.QueryAsync<Contact>(contactsQuery, new { UserId = userId, Ids = contactIds });
                foreach (var note in result)
                {
                    if (note.ContactId == 0) {
                        continue;
                    }
                    var contact = contacts.Where(c => c.Id == note.ContactId).FirstOrDefault();
                    if (contact == null) {
                        continue;
                    }
                    note.Contact = contact;
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
                foreach (var note in result)
                {
                    if (note.StaffId == 0) {
                        continue;
                    }
                    var staff = staffs.Where(c => c.Id == note.StaffId).FirstOrDefault();
                    if (staff == null) {
                        continue;
                    }
                    note.Staff = staff;
                }
                var moneyAccountIds = result.Select(t => t.MoneyAccountId).Distinct();
                string moneyAccountsQuery = @"SELECT
                            *
                        FROM `money_account` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var moneyAccounts = await db.Connection.QueryAsync<Account>(moneyAccountsQuery, new { UserId = userId, Ids = moneyAccountIds });
                foreach (var note in result)
                {
                    if (note.MoneyAccountId == 0) {
                        continue;
                    }
                    var moneyAccount = moneyAccounts.Where(c => c.Id == note.MoneyAccountId).FirstOrDefault();
                    if (moneyAccount == null) {
                        continue;
                    }
                    note.MoneyAccount = moneyAccount;
                }

            }
            return result;
        }
    }

    public async Task<ReceivedNote> GetById(int noteId, string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `received_note`
                WHERE Id = @Id AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var results = await db.Connection.QueryAsync<ReceivedNote>(query, new { Id = noteId, UserId = userId });
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
            if (result != null && result.MoneyAccountId != 0) {
                string staffsQuery = @"SELECT
                            *
                        FROM `money_account` 
                        WHERE UserId = @UserId
                            AND ID = @MoneyAccountId
                    ";
                var moneyAccounts = await db.Connection.QueryAsync<Account>(staffsQuery, new { UserId = userId, MoneyAccountId = result.MoneyAccountId });
                var moneyAccount = moneyAccounts.FirstOrDefault();
                result.MoneyAccount = moneyAccount;
            }
            return result;
        }
    }

    public async Task<bool> Remove(int noteId, string userId) {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM `received_note` 
                WHERE Id = @Id AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Id = noteId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> Save(ReceivedNote note) {
        if (note == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (note.Id > 0) {
                string updateCreatedAt = string.Empty;
                if (note.CreatedAt.HasValue) {
                    updateCreatedAt = ",createdAt = @CreatedAt";
                }
                query = @"
                    UPDATE `received_note`
                    SET 
                        contactId = @ContactId
                        ,staffId = @StaffId
                        ,moneyAccountId = @MoneyAccountId
                        ,contactName = @ContactName
                        ,contactPhone = @ContactPhone
                        ,shippingFee = @ShippingFee
                        ,taxType = @TaxType
                        ,tax = @Tax
                        ,netValue = @NetValue
                        ,discount = @Discount
                        ,total = @Total
                        ,paid = @Paid
                        ,totalForeign = @TotalForeign
                        ,deliveryPerson = @DeliveryPerson
                        ,receiver = @Receiver
                        ,foreignCurrency = @ForeignCurrency
                        ,storeId = @StoreId
                        ,discountOnTotal = @DiscountOnTotal
                        ,itemsJson = @ItemsJson
                        " + updateCreatedAt + @"
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                note.CreatedAt = note.CreatedAt.HasValue ? note.CreatedAt.Value : DateTime.Now;
                query = @"INSERT INTO `received_note`
                    (   
                        `contactId`,
                        `staffId`,
                        `moneyAccountId`,
                        `contactName`,
                        `contactPhone`,
                        `shippingFee`,
                        `taxType`,
                        `tax`,
                        `netValue`,
                        `discount`,
                        `total`,
                        `paid`,
                        `totalForeign`,
                        `deliveryPerson`,
                        `receiver`,
                        `foreignCurrency`,
                        `itemsJson`,
                        `createdAt`,
                        `storeId`,
                        `discountOnTotal`,
                        `userId`
                        )
                    VALUES
                        (
                        @ContactId,
                        @StaffId,
                        @MoneyAccountId,
                        @ContactName,
                        @ContactPhone,
                        @ShippingFee,
                        @TaxType,
                        @Tax,
                        @NetValue,
                        @Discount,
                        @Total,
                        @Paid,
                        @TotalForeign,
                        @DeliveryPerson,
                        @Receiver,
                        @ForeignCurrency,
                        @ItemsJson,
                        @CreatedAt,
                        @StoreId,
                        @DiscountOnTotal,
                        @UserId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, note)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, note);
            return postResult > 0 ? note.Id : 0;
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