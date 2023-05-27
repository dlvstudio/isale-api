using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITradeService
{
    Task<IEnumerable<Trade>> GetTrades(string userId, DateTime? dateFrom, DateTime? dateTo, int contactId, int productId, int staffId, int moneyAccountId, int orderId, int debtId, int receivedNoteId, int transferNoteId, int isReceived);

    Task<Trade> GetTrade(int id, string userId);

    Task<bool> RemoveTrade(int id, string userId, bool? saveAccount);

    Task<int> SaveTrade(Trade trade);
}