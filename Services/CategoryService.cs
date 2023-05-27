using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CategoryService : ICategoryService 
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(
        ICategoryRepository categoryRepository
    ) {
        _categoryRepository = categoryRepository;
    }

    public async Task<bool> DeleteCategoriesToDebt(IEnumerable<DebtToCategory> categories, string userId)
    {
        return await _categoryRepository.DeleteCategoriesToDebt(categories, userId);
    }

    public async Task<bool> DeleteCategoriesToTrade(IEnumerable<TradeToCategory> categories, string userId)
    {
        return await _categoryRepository.DeleteCategoriesToTrade(categories, userId);
    }

    public async Task<IEnumerable<DebtToCategory>> GetAllCategoriesToDebt(string userId)
    {
        return await _categoryRepository.GetAllCategoriesToDebt(userId);
    }

    public async Task<IEnumerable<TradeToCategory>> GetAllCategoriesToTrade(string userId)
    {
        return await _categoryRepository.GetAllCategoriesToTrade(userId);
    }

    public async Task<IEnumerable<Category>> GetCategories(string userId)
    {
        return await _categoryRepository.GetCategories(userId);
    }

    public async Task<IEnumerable<DebtToCategory>> GetCategoriesToDebt(int debtId, string userId)
    {
        return await _categoryRepository.GetCategoriesToDebt(debtId, userId);
    }

    public async Task<IEnumerable<TradeToCategory>> GetCategoriesToTrade(int tradeId, string userId)
    {
        return await _categoryRepository.GetCategoriesToTrade(tradeId, userId);
    }

    public async Task<IEnumerable<TradeToCategory>> GetCategoriesToTrade(IEnumerable<int> ids, string userId)
    {
        return await _categoryRepository.GetCategoriesToTrade(ids, userId);
    }

    public async Task<Category> GetCategory(int id, string userId) {
        var post = await _categoryRepository.GetById(id, userId);
        return post;
    }

    public async Task<IEnumerable<Debt>> GetDebtsByCategory(int categoryId, DateTime? dateFrom, DateTime? dateTo, string userId)
    {
        return await _categoryRepository.GetDebtsByCategory(categoryId, 
            dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6), 
            dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3), 
            userId);
    }

    public async Task<IEnumerable<Trade>> GetTradesByCategory(int categoryId, DateTime? dateFrom, DateTime? dateTo, string userId)
    {
        return await _categoryRepository.GetTradesByCategory(categoryId, 
            dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6), 
            dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3), 
            userId);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategory(int categoryId, string userId)
    {
        return await _categoryRepository.GetProductsByCategory(categoryId, 
            userId);
    }

    public async Task<bool> RemoveCategory(int id, string userId) {
        var post = await _categoryRepository.Remove(id, userId);
        return post;
    }

    public async Task<IEnumerable<int>> SaveCategoriesToDebt(IEnumerable<DebtToCategory> categories, int debtId, string userId)
    {
        return await _categoryRepository.SaveCategoriesToDebt(categories, debtId, userId);
    }

    public async Task<IEnumerable<int>> SaveCategoriesToTrade(IEnumerable<TradeToCategory> categories, int tradeId, string userId)
    {
        return await _categoryRepository.SaveCategoriesToTrade(categories, tradeId, userId);
    }

    public async Task<int> SaveCategory(Category product) {
        return await _categoryRepository.SaveCategory(product);
    }
}