using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DebtService : IDebtService
{
    private readonly IDebtRepository _repository;

    public DebtService(
        IDebtRepository repository
    ) {
        _repository = repository;
    }

    public async Task<IEnumerable<Debt>> GetDebts(string userId, DateTime? dateFrom, DateTime? dateTo, int contactId, int productId, int debtType, int orderId, int receivedNoteId, int staffId, int storeId)
    {
        return await _repository.GetDebts(userId, 
            dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6), 
            dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3), contactId, productId, debtType, orderId, receivedNoteId, staffId, storeId);
    }

    public async Task<Debt> GetDebt(int id, string userId) {
        var post = await _repository.GetById(id, userId);
        return post;
    }

    public async Task<bool> RemoveDebt(int id, string userId) {
        var post = await _repository.Remove(id, userId);
        return post;
    }

    public async Task<int> SaveDebt(Debt trade) {
        return await _repository.SaveDebt(trade);
    }
}