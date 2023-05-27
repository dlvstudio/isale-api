using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class ProductRepository : IProductRepository
{

    private readonly IConfiguration _config;

    public ProductRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<Product>> GetProducts(string userId, int storeId, bool isMaterial, int categoryId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM product 
                WHERE UserId = @UserId
                    AND IsMaterial = @IsMaterial
                ORDER BY createdAt DESC";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Product>(query, new { UserId = userId, IsMaterial = isMaterial });
            var ids = result.Select(r => r.Id).ToList();
            if (storeId != 0 && result != null && result.Any())
            {
                var productQuantities = await GetProductQuantities(ids, storeId, userId);
                foreach (var product in result)
                {
                    var productQuantity = productQuantities.FirstOrDefault(t => t.ProductId == product.Id);
                    if (productQuantity != null)
                    {
                        product.StoreQuantity = productQuantity.Quantity;
                    }
                }
            }
            if (categoryId == 0)
            {
                return result;
            }
            if (categoryId != 0 && result != null && result.Any())
            {
                List<Product> productsWithCategory = new List<Product>();
                var tradeToCategories = await this.GetProductCategories(ids, categoryId, userId);
                if (tradeToCategories == null || !tradeToCategories.Any())
                {
                    return productsWithCategory;
                }
                foreach (var product in result)
                {
                    if (tradeToCategories.Any(t => t.TradeId == product.Id))
                    {
                        productsWithCategory.Add(product);
                    }
                }
                return productsWithCategory;
            }
            return result;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByIds(string userId, IEnumerable<int> ids)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM product 
                WHERE UserId = @UserId
                    AND Id IN @Ids";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Product>(query, new { UserId = userId, Ids = ids });
            return result;
        }
    }

    private async Task<IEnumerable<TradeToCategory>> GetProductCategories(List<int> ids, int categoryId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `trade_to_category`
                WHERE 
                    UserId = @UserId
                    AND CategoryId = @CategoryId
                    AND TradeId IN @Ids";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<TradeToCategory>(query, new { UserId = userId, CategoryId = categoryId, Ids = ids });
            return result;
        }
    }

    private async Task<IEnumerable<ProductStoreQuantity>> GetProductQuantities(List<int> ids, int storeId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `product_quantity`
                WHERE 
                    UserId = @UserId
                    AND StoreId = @StoreId
                    AND ProductId IN @Ids";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<ProductStoreQuantity>(query, new { UserId = userId, StoreId = storeId, Ids = ids });
            return result;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsWithExpiry(string userId, int storeId, bool isMaterial, int categoryId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM product 
                WHERE 
                    UserId = @UserId
                    AND ExpiredAt IS NOT NULL
                    AND ExpiredAt IS NOT NULL
                    AND IsMaterial = @IsMaterial
                ORDER BY ExpiredAt ASC";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Product>(query, new { UserId = userId, IsMaterial = isMaterial });
            var ids = result.Select(r => r.Id).ToList();
            if (storeId != 0 && result != null && result.Any())
            {
                var productQuantities = await GetProductQuantities(ids, storeId, userId);
                foreach (var product in result)
                {
                    var productQuantity = productQuantities.FirstOrDefault(t => t.ProductId == product.Id);
                    if (productQuantity != null)
                    {
                        product.StoreQuantity = productQuantity.Quantity;
                    }
                }
            }
            if (categoryId == 0)
            {
                return result;
            }
            if (categoryId != 0 && result != null && result.Any())
            {
                List<Product> productsWithCategory = new List<Product>();
                var tradeToCategories = await this.GetProductCategories(ids, categoryId, userId);
                if (tradeToCategories == null || !tradeToCategories.Any())
                {
                    return productsWithCategory;
                }
                foreach (var product in result)
                {
                    if (tradeToCategories.Any(t => t.TradeId == product.Id))
                    {
                        productsWithCategory.Add(product);
                    }
                }
                return productsWithCategory;
            }
            return result;
        }
    }

    public async Task<IEnumerable<Product>> GetProductsWithQuantity(string userId, int storeId, bool isMaterial, int categoryId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `product` 
                WHERE 
                    `UserId` = @UserId
                    AND `IsMaterial` = @IsMaterial
                ORDER BY `CreatedAt` DESC";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Product>(query, new { UserId = userId, IsMaterial = isMaterial });
            var ret = new List<Product>();
            if (result != null && result.Any())
            {
                var ids = result.Select(r => r.Id).ToList();
                var productQuantities = storeId != 0
                    ? await GetProductQuantities(ids, storeId, userId)
                    : null;
                foreach (var product in result)
                {
                    var productQuantity = productQuantities != null && productQuantities.Any()
                        ? productQuantities.FirstOrDefault(t => t.ProductId == product.Id)
                        : null;
                    if (productQuantity != null && productQuantity.Quantity > 0)
                    {
                        product.StoreQuantity = productQuantity.Quantity;
                        ret.Add(product);
                    }
                    else if (product.Count > 0)
                    {
                        ret.Add(product);
                    }
                }

                if (categoryId == 0)
                {
                    return ret;
                }
                if (categoryId != 0 && ret != null && ret.Any())
                {
                    List<Product> productsWithCategory = new List<Product>();
                    var tradeToCategories = await this.GetProductCategories(ids, categoryId, userId);
                    if (tradeToCategories == null || !tradeToCategories.Any())
                    {
                        return productsWithCategory;
                    }
                    foreach (var product in ret)
                    {
                        if (tradeToCategories.Any(t => t.TradeId == product.Id))
                        {
                            productsWithCategory.Add(product);
                        }
                    }
                    return productsWithCategory;
                }
            }
            return ret;
        }
    }

    public async Task<Product> GetById(int productId, string userId, int storeId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM product 
                WHERE Id = @Id AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Product>(query, new { Id = productId, UserId = userId });
            if (storeId != 0 && result != null && result.Any())
            {
                var ids = result.Select(r => r.Id).ToList();
                var productQuantities = await GetProductQuantities(ids, storeId, userId);
                foreach (var product in result)
                {
                    var productQuantity = productQuantities.FirstOrDefault(t => t.ProductId == product.Id);
                    if (productQuantity != null)
                    {
                        product.StoreQuantity = productQuantity.Quantity;
                    }
                }
            }
            return result.FirstOrDefault();
        }
    }

    public async Task<ProductStoreQuantity> GetProductStoreQuantity(int productId, int storeId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM product_quantity 
                WHERE ProductId = @Id AND UserId = @UserId AND StoreId = @storeId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<ProductStoreQuantity>(query, new { Id = productId, UserId = userId, StoreId = storeId });
            return result.FirstOrDefault();
        }
    }

    public async Task<Product> GetByCode(string productCode, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM product 
                WHERE `Code` = @Code AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Product>(query, new { Code = productCode, UserId = userId });
            return result.FirstOrDefault();
        }
    }

    public async Task<bool> Remove(int productId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM product 
                WHERE Id = @Id AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Id = productId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> SaveProduct(Product product, bool isUpdatingDate)
    {
        if (product == null)
        {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (product.Id > 0)
            {
                if (!isUpdatingDate)
                {
                    product.ModifiedAt = DateTime.Now;
                }
                query = @"
                    UPDATE `product`
                    SET 
                        code = @Code
                        ,barcode = @Barcode
                        ,count = @Count
                        ,price = @Price
                        ,originalPrice = @OriginalPrice
                        ,isSale = @IsSale
                        ,modifiedAt = @ModifiedAt
                        ,imageUrlsJson = @ImageUrlsJson
                        ,avatarUrl = @AvatarUrl
                        ,unit = @Unit
                        ,unitsJson = @UnitsJson
                        ,title = @Title
                        ,costPrice = @CostPrice
                        ,costPriceForeign = @CostPriceForeign
                        ,foreignCurrency = @ForeignCurrency
                        ,isOption = @IsOption
                        ,isCombo = @IsCombo
                        ,isPublic = @IsPublic
                        ,isService = @IsService
                        ,status = @Status
                        ,itemsJson = @ItemsJson
                        ,materialsJson = @MaterialsJson
                        ,showOnWeb = @ShowOnWeb
                        ,isHotProduct = @IsHotProduct
                        ,isNewProduct = @IsNewProduct
                        ,showPriceOnWeb = @ShowPriceOnWeb
                        ,description = @Description
                        ,expiredAt = @ExpiredAt
                        ,isMaterial = @IsMaterial
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            }
            else
            {
                isInsert = true;
                if (!isUpdatingDate)
                {
                    product.CreatedAt = DateTime.Now;
                    product.ModifiedAt = product.CreatedAt;
                }
                query = @"INSERT INTO `product`
                    (   
                        `title`,
                        `code`,
                        `barcode`,
                        `count`,
                        `price`,
                        `originalPrice`,
                        `isSale`,
                        `imageUrlsJson`,
                        `avatarUrl`,
                        `unit`,
                        `unitsJson`,
                        `costPrice`,
                        `costPriceForeign`,
                        `foreignCurrency`,
                        `userId`,
                        `isOption`
                        ,`isCombo`
                        ,`isPublic`
                        ,`isService`
                        ,`status`
                        ,`itemsJson`
                        ,`materialsJson`
                        ,`showOnWeb`
                        ,`showPriceOnWeb`
                        ,`isHotProduct`
                        ,`isNewProduct`
                        ,`description`
                        ,`expiredAt`
                        ,`isMaterial`
                        )
                    VALUES
                        (
                        @Title,
                        @Code,
                        @Barcode,
                        @Count,
                        @Price,
                        @OriginalPrice,
                        @IsSale,
                        @ImageUrlsJson,
                        @AvatarUrl,
                        @Unit,
                        @UnitsJson,
                        @CostPrice,
                        @CostPriceForeign,
                        @ForeignCurrency,
                        @UserId,
                        @IsOption
                        ,@IsCombo
                        ,@IsPublic
                        ,@IsService
                        ,@Status
                        ,@ItemsJson
                        ,@MaterialsJson
                        ,@ShowOnWeb
                        ,@ShowPriceOnWeb
                        ,@IsHotProduct
                        ,@IsNewProduct
                        ,@Description
                        ,@ExpiredAt
                        ,@IsMaterial
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert)
            {
                var insertResult = (await db.Connection.QueryAsync<int>(query, product)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, product);
            return postResult > 0 ? product.Id : 0;
        }
    }

    public async Task<int> SaveProductStoreQuantity(ProductStoreQuantity product, bool isUpdatingDate)
    {
        if (product == null)
        {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (product.Id > 0)
            {
                query = @"
                    UPDATE `product_quantity`
                    SET 
                        quantity = @Quantity
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                        AND storeId = @StoreId
                ";
            }
            else
            {
                isInsert = true;
                query = @"INSERT INTO `product_quantity`
                    (   
                        `quantity`,
                        `userId`,
                        `storeId`,
                        `productId`
                        )
                    VALUES
                        (
                        @Quantity,
                        @UserId,
                        @StoreId,
                        @ProductId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert)
            {
                var insertResult = (await db.Connection.QueryAsync<int>(query, product)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, product);
            return postResult > 0 ? product.Id : 0;
        }
    }

    public async Task<ProductNote> GetProductNoteById(int productNoteId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM product_note 
                WHERE Id = @Id AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<ProductNote>(query, new { Id = productNoteId, UserId = userId });
            return result.FirstOrDefault();
        }
    }

    public async Task<IEnumerable<ProductNote>> GetNotes(string userId, DateTime dateFrom, DateTime dateTo, int contactId, int productId, int orderId, int receivedNoteId, int tradeId, int storeId, int transferNoteId, int staffId, bool withStaff = false)
    {
        var dateFromOnlyDate = dateFrom.Date.ToString("yyyy-MM-dd");
        var dateToAddOne = dateTo.Date.AddDays(1).ToString("yyyy-MM-dd");
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM `product_note` 
                WHERE UserId = @UserId
                    AND createdAt >= @DateFrom 
                    AND createdAt < @DateTo 
                    AND (@ContactId = 0 OR ContactId = @ContactId) 
                    AND (@ProductId = 0 OR ProductId = @ProductId) 
                    AND (@TransferNoteId = 0 OR TransferNoteId = @TransferNoteId) 
                    AND (@OrderId = 0 OR OrderId = @OrderId) 
                    AND (@TradeId = 0 OR TradeId = @TradeId) 
                    AND (@StoreId = 0 OR StoreId = @StoreId) 
                    AND (@ReceivedNoteId = 0 OR ReceivedNoteId = @ReceivedNoteId) 
                    AND (@StaffId = 0 OR (OrderId = 0 AND ReceivedNoteId = 0 AND TransferNoteId = 0)
                        OR TradeId IN (SELECT Id FROM `trade` WHERE StaffID = @StaffId AND UserId = @UserId)
                        OR OrderId IN (SELECT Id FROM `order` WHERE StaffID = @StaffId AND UserId = @UserId)
                        OR TransferNoteId IN (SELECT Id FROM `transfer_note` WHERE StaffID = @StaffId AND UserId = @UserId)
                        OR ReceivedNoteId IN (SELECT Id FROM `received_note` WHERE StaffID = @StaffId AND UserId = @UserId)
                    ) 
                ORDER BY CreatedAt DESC, Id DESC
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<ProductNote>(query, new { UserId = userId, DateFrom = dateFromOnlyDate, DateTo = dateToAddOne, ContactId = contactId, ProductId = productId, OrderId = orderId, ReceivedNoteId = receivedNoteId, TradeId = tradeId, StoreId = storeId, TransferNoteId = transferNoteId, StaffId = staffId });
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
                foreach (var trade in result)
                {
                    if (trade.ContactId == 0)
                    {
                        continue;
                    }
                    var contact = contacts.Where(c => c.Id == trade.ContactId).FirstOrDefault();
                    if (contact == null)
                    {
                        continue;
                    }
                    trade.Contact = contact;
                }
            }
            if (result != null && result.Any())
            {
                var productIds = result.Select(t => t.ProductId).Distinct();
                string productsQuery = @"SELECT
                            *
                        FROM product 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var products = await db.Connection.QueryAsync<Product>(productsQuery, new { UserId = userId, Ids = productIds });
                foreach (var note in result)
                {
                    if (note.ProductId == 0)
                    {
                        continue;
                    }
                    var product = products.Where(c => c.Id == note.ProductId).FirstOrDefault();
                    if (product == null)
                    {
                        continue;
                    }
                    note.Product = product;
                    note.ProductCode = product.Code;
                    note.ProductName = product.Title;
                }
            }
            if (result != null && result.Any())
            {
                var orderIds = result.Select(t => t.OrderId).Distinct();
                string ordersQuery = @"SELECT
                            *
                        FROM `order` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var orders = await db.Connection.QueryAsync<Order>(ordersQuery, new { UserId = userId, Ids = orderIds });
                IEnumerable<Staff> staffs = new List<Staff>();
                if (withStaff)
                {
                    var staffIds = orders != null && orders.Any()
                        ? orders.Where(o => o.StaffId > 0).Select(o => o.StaffId).ToList()
                        : new List<int>();
                    string staffsQuery = @"SELECT
                            *
                        FROM `staff` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                    staffs = await db.Connection.QueryAsync<Staff>(staffsQuery, new { UserId = userId, Ids = staffIds });
                }
                foreach (var note in result)
                {
                    if (note.OrderId == 0)
                    {
                        continue;
                    }
                    var item = orders.Where(c => c.Id == note.OrderId).FirstOrDefault();
                    if (item == null)
                    {
                        continue;
                    }
                    note.Order = item;
                    if (withStaff)
                    {
                        var staff = staffs.Where(c => c.Id == note.Order.StaffId).FirstOrDefault();
                        if (staff == null)
                        {
                            continue;
                        }
                        note.Order.Staff = staff;
                    }
                }
                var receivedNoteIds = result.Select(t => t.ReceivedNoteId).Distinct();
                string receivedNotesQuery = @"SELECT
                            *
                        FROM `received_note` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var receivedNotes = await db.Connection.QueryAsync<ReceivedNote>(receivedNotesQuery, new { UserId = userId, Ids = receivedNoteIds });
                foreach (var note in result)
                {
                    if (note.ReceivedNoteId == 0)
                    {
                        continue;
                    }
                    var item = receivedNotes.Where(c => c.Id == note.ReceivedNoteId).FirstOrDefault();
                    if (item == null)
                    {
                        continue;
                    }
                    note.ReceivedNote = item;
                }

                var tradeIds = result.Select(t => t.TradeId).Distinct();
                string tradesQuery = @"SELECT
                            *
                        FROM `trade` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
                var trades = await db.Connection.QueryAsync<Trade>(tradesQuery, new { UserId = userId, Ids = tradeIds });
                foreach (var note in result)
                {
                    if (note.TradeId == 0)
                    {
                        continue;
                    }
                    var item = trades.Where(c => c.Id == note.TradeId).FirstOrDefault();
                    if (item == null)
                    {
                        continue;
                    }
                    note.Trade = item;
                }
            }

            var transferNoteIds = result.Select(t => t.TransferNoteId).Distinct();
            string transferNotesQuery = @"SELECT
                            *
                        FROM `transfer_note` 
                        WHERE UserId = @UserId
                            AND ID IN @Ids
                    ";
            var transferNotes = await db.Connection.QueryAsync<TransferNote>(transferNotesQuery, new { UserId = userId, Ids = transferNoteIds });
            foreach (var note in result)
            {
                if (note.TransferNoteId == 0)
                {
                    continue;
                }
                var item = transferNotes.Where(c => c.Id == note.TransferNoteId).FirstOrDefault();
                if (item == null)
                {
                    continue;
                }
                note.TransferNote = item;
            }
            return result;
        }
    }

    public async Task<bool> RemoveProductNote(int productNoteId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM `product_note` 
                WHERE Id = @Id AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Id = productNoteId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> SaveProductNote(ProductNote note)
    {
        if (note == null)
        {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (note.Id > 0)
            {
                note.ModifiedAt = DateTime.Now;
                query = @"
                    UPDATE `product_note`
                    SET
                        `receivedNoteId` = @ReceivedNoteId,
                        `transferNoteId` = @TransferNoteId,
                        `orderId` = @OrderId,
                        `tradeId` = @TradeId,
                        `productId` = @ProductId,
                        `contactId` = @ContactId,
                        `productCode` = @ProductCode,
                        `productName` = @ProductName,
                        `note` = @Note,
                        `unitPriceForeign` = @UnitPriceForeign,
                        `unitPrice` = @UnitPrice,
                        `foreignCurrency` = @ForeignCurrency,
                        `unit` = @Unit,
                        `basicUnit` = @BasicUnit,
                        `unitExchange` = @UnitExchange,
                        `quantity` = @Quantity,
                        `amountForeign` = @AmountForeign,
                        `amount` = @Amount,
                        `discount` = @Discount,
                        `discountType` = @DiscountType,
                        `receivedDate` = @ReceivedDate,
                        `createdAt` = @CreatedAt,
                        `storeId` = @StoreId,
                        `modifiedAt` = @ModifiedAt
                    WHERE
                        id = @Id 
                        AND userId = @UserId
                ";
            }
            else
            {
                isInsert = true;
                note.ModifiedAt = DateTime.Now;
                query = @"
                    INSERT INTO `product_note`
                        (
                        `receivedNoteId`,
                        `transferNoteId`,
                        `orderId`,
                        `tradeId`,
                        `productId`,
                        `contactId`,
                        `productCode`,
                        `productName`,
                        `note`,
                        `unitPriceForeign`,
                        `unitPrice`,
                        `foreignCurrency`,
                        `unit`,
                        `basicUnit`,
                        `unitExchange`,
                        `quantity`,
                        `amountForeign`,
                        `amount`,
                        `discount`,
                        `discountType`,
                        `userId`,
                        `receivedDate`,
                        `createdAt`,
                        `storeId`,
                        `modifiedAt`)
                    VALUES
                        (
                        @ReceivedNoteId,
                        @TransferNoteId,
                        @OrderId,
                        @TradeId,
                        @ProductId,
                        @ContactId,
                        @ProductCode,
                        @ProductName,
                        @note,
                        @UnitPriceForeign,
                        @UnitPrice,
                        @ForeignCurrency,
                        @Unit,
                        @BasicUnit,
                        @UnitExchange,
                        @Quantity,
                        @AmountForeign,
                        @Amount,
                        @Discount,
                        @DiscountType,
                        @UserId,
                        @ReceivedDate,
                        @CreatedAt,
                        @StoreId,
                        @ModifiedAt)
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert)
            {
                var insertResult = (await db.Connection.QueryAsync<int>(query, note)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, note);
            return postResult > 0 ? note.Id : 0;
        }
    }

    public async Task<Product> SearchProductByBarcode(string barcode, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM product 
                WHERE Barcode = @Barcode AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Product>(query, new { Barcode = barcode, UserId = userId });
            return result.FirstOrDefault();
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