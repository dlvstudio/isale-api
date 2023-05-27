using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class CategoryRepository: ICategoryRepository {

    private readonly IConfiguration _config;

    public CategoryRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IEnumerable<Category>> GetCategories(string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM trade_category 
                WHERE UserId = @UserId
                ORDER BY OrderIndex ASC
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Category>(query, new { UserId = userId });
            return result;
        }
    }

    public async Task<Category> GetById(int productId, string userId) {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM trade_category 
                WHERE Id = @Id AND UserId = @UserId
                LIMIT 1";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Category>(query, new { Id = productId, UserId = userId });
            return result.FirstOrDefault();
        }
    }

    public async Task<bool> Remove(int productId, string userId) {
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM trade_category 
                WHERE Id = @Id AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Id = productId, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<int> SaveCategory(Category product) {
        if (product == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (product.Id > 0) {
                product.ModifiedAt = DateTime.Now;
                query = @"
                    UPDATE `trade_category`
                    SET 
                        title = @Title
                        ,modifiedAt = @ModifiedAt
                        ,orderIndex = @OrderIndex
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                product.CreatedAt = DateTime.Now;
                product.ModifiedAt = product.CreatedAt;
                query = @"INSERT INTO `trade_category`
                    (   
                        `title`,
                        `userId`,
                        `orderIndex`
                        )
                    VALUES
                        (
                        @Title,
                        @UserId,
                        @OrderIndex
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, product)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, product);
            return postResult > 0 ? product.Id : 0;
        }
    }

    public async Task<IEnumerable<TradeToCategory>> GetCategoriesToTrade(int tradeId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM trade_to_category 
                WHERE UserId = @UserId
                    AND tradeId = @TradeId
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<TradeToCategory>(query, new { UserId = userId, TradeId = tradeId });
            await BindTradeToCategories(result, userId);
            return result;
        }
    }

    public async Task<IEnumerable<TradeToCategory>> GetCategoriesToTrade(IEnumerable<int> ids, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM trade_to_category 
                WHERE UserId = @UserId
                    AND tradeId IN @Ids
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<TradeToCategory>(query, new { UserId = userId, Ids = ids });
            await BindTradeToCategories(result, userId);
            return result;
        }
    }

    public async Task<IEnumerable<TradeToCategory>> GetAllCategoriesToTrade(string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM trade_to_category 
                WHERE UserId = @UserId
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<TradeToCategory>(query, new { UserId = userId });
            await BindTradeToCategories(result, userId);
            return result;
        }
    }

    private async Task BindTradeToCategories(IEnumerable<TradeToCategory> tradeToCategories, string userId) {
        if (tradeToCategories == null || !tradeToCategories.Any()) {
            return;
        }
        var tradeIds = tradeToCategories.Select(t => t.TradeId).ToList();
        var categoryIds = tradeToCategories.Select(t => t.CategoryId).ToList();
        using (var db = AppDb)
        {
            string queryTrades = @"SELECT
                    *
                FROM trade 
                WHERE UserId = @UserId
                    AND Id IN @TradeIds
                ";
            string queryCategories = @"SELECT
                    *
                FROM trade_category 
                WHERE UserId = @UserId
                    AND Id IN @CategoryIds
                ORDER BY OrderIndex ASC
                ";
            await db.Connection.OpenAsync();
            var trades = await db.Connection.QueryAsync<Trade>(queryTrades, new { UserId = userId, TradeIds = tradeIds });
            var categories = await db.Connection.QueryAsync<Category>(queryCategories, new { UserId = userId, CategoryIds = categoryIds });
            foreach (var tradeToCategory in tradeToCategories)
            {
                tradeToCategory.Trade = trades.FirstOrDefault(t => t.Id == tradeToCategory.TradeId);
                tradeToCategory.TradeCategory = categories.FirstOrDefault(t => t.Id == tradeToCategory.CategoryId);
            }
        }
    }

    private async Task BindDebtToCategories(IEnumerable<DebtToCategory> tradeToCategories, string userId) {
        if (tradeToCategories == null || !tradeToCategories.Any()) {
            return;
        }
        var tradeIds = tradeToCategories.Select(t => t.DebtId).ToList();
        var categoryIds = tradeToCategories.Select(t => t.CategoryId).ToList();
        using (var db = AppDb)
        {
            string queryTrades = @"SELECT
                    *
                FROM debt 
                WHERE UserId = @UserId
                    AND Id IN @TradeIds
                ";
            string queryCategories = @"SELECT
                    *
                FROM trade_category 
                WHERE UserId = @UserId
                    AND Id IN @CategoryIds
                ORDER BY OrderIndex ASC
                ";
            await db.Connection.OpenAsync();
            var trades = await db.Connection.QueryAsync<Debt>(queryTrades, new { UserId = userId, TradeIds = tradeIds });
            var categories = await db.Connection.QueryAsync<Category>(queryCategories, new { UserId = userId, CategoryIds = categoryIds });
            foreach (var tradeToCategory in tradeToCategories)
            {
                tradeToCategory.Debt = trades.FirstOrDefault(t => t.Id == tradeToCategory.DebtId);
                tradeToCategory.TradeCategory = categories.FirstOrDefault(t => t.Id == tradeToCategory.CategoryId);
            }
        }
    }

    public async Task<IEnumerable<Product>> GetProductsByCategory(int categoryId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM product
                WHERE UserId = @UserId
                    AND ID IN (
                        SELECT ttc.tradeId FROM trade_to_category ttc
                        WHERE  ttc.categoryId = @CategoryId 
                            AND userId = @UserId
                    )
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Product>(query, new { UserId = userId, 
                CategoryId = categoryId});
            return result;
        }
    }

    public async Task<IEnumerable<Trade>> GetTradesByCategory(int categoryId, DateTime dateFrom, DateTime dateTo, string userId)
    {
        using (var db = AppDb)
        {
            var dateFromOnlyDate = dateFrom.Date.ToString("yyyy-MM-dd");
            var dateToAddOne = dateTo.Date.AddDays(1).ToString("yyyy-MM-dd");
            string query = @"SELECT
                    *
                FROM trade
                WHERE UserId = @UserId
                    AND createdAt >= @DateFrom 
                    AND createdAt < @DateTo 
                    AND ID IN (
                        SELECT ttc.tradeId FROM trade_to_category ttc
                        WHERE  ttc.categoryId = @CategoryId 
                            AND userId = @UserId
                    )
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Trade>(query, new { UserId = userId, 
                CategoryId = categoryId, 
                DateFrom = dateFromOnlyDate, 
                DateTo = dateToAddOne });
            return result;
        }
    }

    private async Task<int> SaveCategoryToTrade(TradeToCategory category, int tradeId, string userId) {
        if (category == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (category.Id > 0) {
                category.TradeId = tradeId;
                category.UserId = userId;
                query = @"
                    UPDATE `trade_to_category`
                    SET 
                        tradeId = @TradeId,
                        categoryId = @CategoryId
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                category.UserId = userId;
                category.TradeId = tradeId;
                query = @"INSERT INTO `trade_to_category`
                    (   
                        `tradeId`,
                        `categoryId`,
                        `userId`
                        )
                    VALUES
                        (
                        @TradeId,
                        @CategoryId,
                        @UserId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, category)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, category);
            return postResult > 0 ? category.Id : 0;
        }
    }

    private async Task<int> SaveCategoryToDebt(DebtToCategory category, int debtId, string userId) {
        if (category == null) {
            return 0;
        }
        using (var db = AppDb)
        {
            string query = string.Empty;
            var isInsert = false;
            if (category.Id > 0) {
                category.DebtId = debtId;
                category.UserId = userId;
                query = @"
                    UPDATE `debt_to_category`
                    SET 
                        debtId = @DebtId,
                        categoryId = @CategoryId
                    WHERE 
                        id = @Id 
                        AND userId = @UserId
                ";
            } else {
                isInsert = true;
                category.UserId = userId;
                category.DebtId = debtId;
                query = @"INSERT INTO `debt_to_category`
                    (   
                        `debtId`,
                        `categoryId`,
                        `userId`
                        )
                    VALUES
                        (
                        @DebtId,
                        @CategoryId,
                        @UserId
                        )
                    ;
                    SELECT LAST_INSERT_ID();
                ";
            }
            await db.Connection.OpenAsync();
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, category)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, category);
            return postResult > 0 ? category.Id : 0;
        }
    }

    public async Task<IEnumerable<int>> SaveCategoriesToTrade(IEnumerable<TradeToCategory> categories, int tradeId, string userId)
    {
        var res = new List<int>();
        foreach (var category in categories)
        {
            var id = await SaveCategoryToTrade(category, tradeId, userId);
            res.Add(id);
        }
        return res;
    }

    public async Task<bool> DeleteCategoriesToTrade(IEnumerable<TradeToCategory> categories, string userId)
    {
        var ids = categories.Select(t => t.Id).ToList();
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM trade_to_category 
                WHERE Id IN @Ids AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Ids = ids, UserId = userId });
            return postResult > 0;
        }
    }

    public async Task<IEnumerable<DebtToCategory>> GetCategoriesToDebt(int debtId, string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM debt_to_category 
                WHERE UserId = @UserId
                    AND debtId = @DebtId
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<DebtToCategory>(query, new { UserId = userId, DebtId = debtId });
            await BindDebtToCategories(result, userId);
            return result;
        }
    }

    public async Task<IEnumerable<DebtToCategory>> GetAllCategoriesToDebt(string userId)
    {
        using (var db = AppDb)
        {
            string query = @"SELECT
                    *
                FROM debt_to_category 
                WHERE UserId = @UserId
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<DebtToCategory>(query, new { UserId = userId });
            await BindDebtToCategories(result, userId);
            return result;
        }
    }

    public async Task<IEnumerable<Debt>> GetDebtsByCategory(int categoryId, DateTime dateFrom, DateTime dateTo, string userId)
    {
        using (var db = AppDb)
        {
            var dateFromOnlyDate = dateFrom.Date.ToString("yyyy-MM-dd");
            var dateToAddOne = dateTo.Date.AddDays(1).ToString("yyyy-MM-dd");
            string query = @"SELECT
                    *
                FROM debt
                WHERE UserId = @UserId
                    AND createdAt >= @DateFrom 
                    AND createdAt < @DateTo 
                    AND ID IN (
                        SELECT ttc.debtId FROM debt_to_category ttc
                        WHERE  ttc.categoryId = @CategoryId 
                            AND userId = @UserId
                    )
                ";
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<Debt>(query, new { UserId = userId, 
                CategoryId = categoryId, 
                DateFrom = dateFromOnlyDate, 
                DateTo = dateToAddOne });
            return result;
        }
    }

    public async Task<IEnumerable<int>> SaveCategoriesToDebt(IEnumerable<DebtToCategory> categories, int debtId, string userId)
    {
        var ret = new List<int>();
        foreach (var category in categories)
        {
            ret.Add(await SaveCategoryToDebt(category, debtId, userId));
        }
        return ret;
    }

    public async Task<bool> DeleteCategoriesToDebt(IEnumerable<DebtToCategory> categories, string userId)
    {
        var ids = categories.Select(t => t.Id).ToList();
        using (var db = AppDb)
        {
            string query = @"DELETE
                FROM debt_to_category 
                WHERE Id IN @Ids AND UserId = @UserId";
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, new { Ids = ids, UserId = userId });
            return postResult > 0;
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