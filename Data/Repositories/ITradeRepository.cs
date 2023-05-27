using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITradeRepository {

    Task<IEnumerable<Trade>> GetTrades(string userId, DateTime dateFrom, DateTime dateTo, int contactId, int productId, int staffId, int moneyAccountId, int orderId, int debtId, int receivedNoteId, int transferNoteId, int isReceived);

    Task<Trade> GetById(int tradeId, string userId);

    Task<bool> Remove(int tradeId, string userId);

    Task<int> SaveTrade(Trade trade);
}