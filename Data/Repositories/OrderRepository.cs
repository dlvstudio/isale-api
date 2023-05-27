using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class OrderRepository : IOrderRepository
{

    private readonly IConfiguration _config;

    public OrderRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<Order>> GetOrders(string userId, DateTime dateFrom, DateTime dateTo, int contactId, int staffId, int storeId, int? status, IEnumerable<int> orderIds = null)
    {
        var dateFromOnlyDate = dateFrom.Date.ToString("yyyy-MM-dd");
        var dateToAddOne = dateTo.Date.AddDays(1).ToString("yyyy-MM-dd");
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `order` 
                WHERE UserId = @UserId
                    AND (@ContactId = 0 OR ContactId = @ContactId) 
                    AND (@StaffId = 0 OR StaffId = @StaffId) 
                    AND (@StoreId = 0 OR StoreId = @StoreId) 
                    AND (@Status = -1 OR Status = @Status)"
                    + (orderIds != null && orderIds.Any() ? " AND (ID IN @OrderIds) " : string.Empty)
                    + @"
                    AND createdAt >= @DateFrom 
                    AND createdAt < @DateTo 
                    AND ItemsJson IS NOT NULL AND TRIM(ItemsJson) <> ''
                ORDER BY createdAt DESC
                ";
            await db.Connection.OpenAsync();
            var queryParams = new { UserId = userId, DateFrom = dateFromOnlyDate, DateTo = dateToAddOne, ContactId = contactId, StaffId = staffId, StoreId = storeId, Status = (status.HasValue ? status.Value : -1) };
            var queryParams2 = new { UserId = userId, DateFrom = dateFromOnlyDate, DateTo = dateToAddOne, ContactId = contactId, StaffId = staffId, StoreId = storeId, Status = (status.HasValue ? status.Value : -1), OrderIds = orderIds };
            var result = orderIds != null && orderIds.Any()
                ? (await db.Connection.QueryAsync<Order>(query, queryParams2))
                : (await db.Connection.QueryAsync<Order>(query, queryParams));

            if (result != null && result.Any())
            {
                var contactIds = result.Select(t => t.ContactId).Distinct();
                string contactsQuery = @"SELECT
                            *
                        FROM contact 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var contacts = await db.Connection.QueryAsync<Contact>(contactsQuery, new { UserId = userId, Ids = contactIds });
                foreach (var order in result)
                {
                    if (order.ContactId == 0)
                    {
                        continue;
                    }
                    var contact = contacts.Where(c => c.Id == order.ContactId).FirstOrDefault();
                    if (contact == null)
                    {
                        continue;
                    }
                    order.Contact = contact;
                }
            }
            if (result != null && result.Any())
            {
                var ids = result.Select(t => t.TableId).Distinct();
                string recordQuery = @"SELECT
                            *
                        FROM `table`
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var records = await db.Connection.QueryAsync<ShopTable>(recordQuery, new { UserId = userId, Ids = ids });
                foreach (var order in result)
                {
                    if (order.TableId == 0)
                    {
                        continue;
                    }
                    var table = records.Where(c => c.Id == order.TableId).FirstOrDefault();
                    if (table == null)
                    {
                        continue;
                    }
                    order.Table = table;
                }
            }
            if (result != null && result.Any())
            {
                var staffIds = result.Select(t => t.StaffId).Distinct();
                string staffsQuery = @"SELECT
                            *
                        FROM `staff` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var staffs = await db.Connection.QueryAsync<Staff>(staffsQuery, new { UserId = userId, Ids = staffIds });
                foreach (var order in result)
                {
                    if (order.StaffId == 0)
                    {
                        continue;
                    }
                    var staff = staffs.Where(c => c.Id == order.StaffId).FirstOrDefault();
                    if (staff == null)
                    {
                        continue;
                    }
                    order.Staff = staff;
                }
                var moneyAccountIds = result.Select(t => t.MoneyAccountId).Distinct();
                string moneyAccountsQuery = @"SELECT
                            *
                        FROM `money_account` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var moneyAccounts = await db.Connection.QueryAsync<Account>(moneyAccountsQuery, new { UserId = userId, Ids = moneyAccountIds });
                foreach (var order in result)
                {
                    if (order.MoneyAccountId == 0)
                    {
                        continue;
                    }
                    var moneyAccount = moneyAccounts.Where(c => c.Id == order.MoneyAccountId).FirstOrDefault();
                    if (moneyAccount == null)
                    {
                        continue;
                    }
                    order.MoneyAccount = moneyAccount;
                }
            }
            return result;
        }
    }

    public async Task<Order> GetByCode(string orderCode, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `order`
                WHERE OrderCode = @OrderCode AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var results = await db.Connection.QueryAsync<Order>(query, new { OrderCode = orderCode, UserId = userId });
            var result = results.FirstOrDefault();
            if (result != null && result.ContactId != 0)
            {
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
            if (result != null && result.StaffId != 0)
            {
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
            if (result != null && result.MoneyAccountId != 0)
            {
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

    public async Task<Order> GetById(int orderId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `order`
                WHERE Id = @Id AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var results = await db.Connection.QueryAsync<Order>(query, new { Id = orderId, UserId = userId });
            var result = results.FirstOrDefault();
            if (result != null && result.ContactId != 0)
            {
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
            if (result != null && result.StaffId != 0)
            {
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
            if (result != null && result.TableId != 0)
            {
                string recordQuery = @"SELECT
                            *
                        FROM `table`
                        WHERE UserId = @UserId
                            AND ID = @Id
                    ";
                var records = await db.Connection.QueryAsync<ShopTable>(recordQuery, new { UserId = userId, Id = result.TableId });
                var table = records.FirstOrDefault();
                result.Table = table;
            }
            if (result != null && result.MoneyAccountId != 0)
            {
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

    public async Task<bool> Remove(int orderId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM `order` 
                WHERE Id = @Id AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Id = orderId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> SaveOrder(Order order)
    {
        if (order == null)
        {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (order.Id > 0)
            {
                string updateCreatedAt = string.Empty;
                if (order.CreatedAt.HasValue)
                {
                    updateCreatedAt = ",createdAt = @CreatedAt";
                }
                query = @"
                    UPDATE `order`
                    SET 
                        orderCode = @OrderCode
                        ,contactId = @ContactId
                        ,staffId = @StaffId
                        ,moneyAccountId = @MoneyAccountId
                        ,contactName = @ContactName
                        ,contactPhone = @ContactPhone
                        ,contactAddress = @ContactAddress
                        ,shippingFee = @ShippingFee
                        ,taxType = @TaxType
                        ,tax = @Tax
                        ,netValue = @NetValue
                        ,discount = @Discount
                        ,total = @Total
                        ,paid = @Paid
                        ,`change` = @Change
                        ,`status` = @Status
                        ,itemsJson = @ItemsJson
                        ,tableId = @TableId
                        ,storeId = @StoreId
                        ,note = @Note
                        ,billOfLadingCode = @BillOfLadingCode
                        ,shippingPartner = @ShippingPartner
                        ,shipperName = @ShipperName
                        ,shipperPhone = @ShipperPhone
                        ,shipperId = @ShipperId
                        ,deliveryAddress = @DeliveryAddress
                        ,hasShipInfo = @HasShipInfo
                        ,amountFromPoint = @AmountFromPoint
                        ,pointPaymentExchange = @PointPaymentExchange
                        ,pointAmount = @PointAmount
                        ,shipCostOnCustomer = @ShipCostOnCustomer
                        ,discountOnTotal = @DiscountOnTotal
                        ,totalPromotionDiscount = @TotalPromotionDiscount
                        ,promotionsJson = @PromotionsJson
                        " + updateCreatedAt + @"
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            }
            else
            {
                isInsert = true;
                order.CreatedAt = order.CreatedAt.HasValue ? order.CreatedAt.Value : DateTime.Now;
                query = @"INSERT INTO `order`
                    (   
                        `orderCode`,
                        `contactId`,
                        `staffId`,
                        `moneyAccountId`,
                        `contactName`,
                        `contactPhone`,
                        `contactAddress`,
                        `shippingFee`,
                        `taxType`,
                        `tax`,
                        `netValue`,
                        `discount`,
                        `total`,
                        `paid`,
                        `change`,
                        `status`,
                        `itemsJson`,
                        `createdAt`,
                        `tableId`,
                        `note`,
                        `userId`,
                        `storeId`,
                        `billOfLadingCode`,
                        `shippingPartner`,
                        `shipperName`,
                        `shipperPhone`,
                        `shipperId`,
                        `deliveryAddress`,
                        `hasShipInfo`,
                        `pointAmount`,
                        `pointPaymentExchange`,
                        `amountFromPoint`,
                        `shipCostOnCustomer`,
                        `totalPromotionDiscount`,
                        `promotionsJson`,
                        `discountOnTotal`
                        )
                    VALUES
                        (
                        @OrderCode,
                        @ContactId,
                        @StaffId,
                        @MoneyAccountId,
                        @ContactName,
                        @ContactPhone,
                        @ContactAddress,
                        @ShippingFee,
                        @TaxType,
                        @Tax,
                        @NetValue,
                        @Discount,
                        @Total,
                        @Paid,
                        @Change,
                        @Status,
                        @ItemsJson,
                        @CreatedAt,
                        @TableId,
                        @Note,
                        @UserId,
                        @StoreId,
                        @BillOfLadingCode,
                        @ShippingPartner,
                        @ShipperName,
                        @ShipperPhone,
                        @ShipperId,
                        @DeliveryAddress,
                        @HasShipInfo,
                        @PointAmount,
                        @PointPaymentExchange,
                        @AmountFromPoint,
                        @ShipCostOnCustomer,
                        @TotalPromotionDiscount,
                        @PromotionsJson,
                        @DiscountOnTotal
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert)
            {
                var insertResult = (await db.Connection.QueryAsync<int>(query, order)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, order);
            return postResult > 0 ? order.Id : 0;
        }
    }



    public async Task<int> SaveOrderStatus(Order order)
    {
        if (order == null)
        {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            query = @"
                    UPDATE `order`
                    SET 
                        `status` = @Status
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, order);
            return postResult > 0 ? order.Id : 0;
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