using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICategoryRepository {

    Task<IEnumerable<Category>> GetCategories(string userId);

    Task<Category> GetById(int productId, string userId);

    Task<bool> Remove(int productId, string userId);

    Task<int> SaveCategory(Category product);

    Task<IEnumerable<TradeToCategory>> GetCategoriesToTrade(int tradeId, string userId);
    
    Task<IEnumerable<TradeToCategory>> GetCategoriesToTrade(IEnumerable<int> ids, string userId);

    Task<IEnumerable<DebtToCategory>> GetCategoriesToDebt(int debtId, string userId);

    Task<IEnumerable<TradeToCategory>> GetAllCategoriesToTrade(string userId);

    Task<IEnumerable<DebtToCategory>> GetAllCategoriesToDebt(string userId);

    Task<IEnumerable<Trade>> GetTradesByCategory(int categoryId, DateTime dateFrom, DateTime dateTo, string userId);

    Task<IEnumerable<Product>> GetProductsByCategory(int categoryId, string userId);

    Task<IEnumerable<Debt>> GetDebtsByCategory(int categoryId, DateTime dateFrom, DateTime dateTo, string userId);

    Task<IEnumerable<int>> SaveCategoriesToTrade(IEnumerable<TradeToCategory> categories, int tradeId, string userId);

    Task<IEnumerable<int>> SaveCategoriesToDebt(IEnumerable<DebtToCategory> categories, int tradeId, string userId);

    Task<bool> DeleteCategoriesToTrade(IEnumerable<TradeToCategory> categories, string userId);
    
    Task<bool> DeleteCategoriesToDebt(IEnumerable<DebtToCategory> categories, string userId);
}