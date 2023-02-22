using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDebtService
{
    Task<IEnumerable<Debt>> GetDebts(string userId, DateTime? dateFrom, DateTime? dateTo, int contactId, int productId, int debtType, int orderId, int receivedNoteId, int staffId, int storeId);

    Task<Debt> GetDebt(int id, string userId);

    Task<bool> RemoveDebt(int id, string userId);

    Task<int> SaveDebt(Debt trade);
}