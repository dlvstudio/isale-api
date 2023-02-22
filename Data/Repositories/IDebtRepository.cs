using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDebtRepository {

    Task<IEnumerable<Debt>> GetDebts(string userId, DateTime dateFrom, DateTime dateTo, int contactId, int productId, int debtType, int orderId, int receivedNoteId, int staffId, int storeId);

    Task<Debt> GetById(int debtId, string userId);

    Task<bool> Remove(int debtId, string userId);

    Task<int> SaveDebt(Debt debt);
}