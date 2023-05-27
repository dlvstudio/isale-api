using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TradeService : ITradeService
{
    private readonly ITradeRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IDebtRepository _debtRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IReceivedNoteRepository _receivedNoteRepository;
    private readonly IAccountRepository _accountRepository;

    public TradeService(
        ITradeRepository repository,
        IProductRepository productRepository,
        IDebtRepository debtRepository,
        IOrderRepository orderRepository,
        IAccountRepository accountRepository,
        IReceivedNoteRepository receivedNoteRepository
    )
    {
        _repository = repository;
        _productRepository = productRepository;
        _debtRepository = debtRepository;
        _orderRepository = orderRepository;
        _receivedNoteRepository = receivedNoteRepository;
        _accountRepository = accountRepository;
    }

    public async Task<IEnumerable<Trade>> GetTrades(string userId, DateTime? dateFrom, DateTime? dateTo, int contactId, int productId, int staffId, int moneyAccountId, int orderId, int debtId, int receivedNoteId, int transferNoteId, int isReceived)
    {
        return await _repository.GetTrades(userId,
            dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6),
            dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3), contactId, productId, staffId, moneyAccountId, orderId, debtId, receivedNoteId, transferNoteId, isReceived);
    }

    public async Task<Trade> GetTrade(int id, string userId)
    {
        var post = await _repository.GetById(id, userId);
        return post;
    }

    public async Task<bool> RemoveTrade(int id, string userId, bool? saveAccount)
    {
        var oldTrade = await _repository.GetById(id, userId);
        if (oldTrade.MoneyAccountId != 0 && saveAccount.HasValue && saveAccount.Value)
        {
            var oldMoneyAccount = await _accountRepository.GetById(oldTrade.MoneyAccountId, oldTrade.UserId);
            oldMoneyAccount.Total -= oldTrade.Value * (oldTrade.IsReceived ? 1 : -1);
            await _accountRepository.SaveAccount(oldMoneyAccount);
        }
        var post = await _repository.Remove(id, userId);
        return post;
    }

    public async Task<int> SaveTrade(Trade trade)
    {
        if (trade.Id > 0)
        {
            if (trade.SaveAccount.HasValue && trade.SaveAccount.Value)
            {
                var oldTrade = await _repository.GetById(trade.Id, trade.UserId);
                if (oldTrade != null && oldTrade.MoneyAccountId != 0 && trade.MoneyAccountId != oldTrade.MoneyAccountId)
                {
                    var oldMoneyAccount = await _accountRepository.GetById(oldTrade.MoneyAccountId, oldTrade.UserId);
                    oldMoneyAccount.Total -= oldTrade.Value * (oldTrade.IsReceived ? 1 : -1);
                    await _accountRepository.SaveAccount(oldMoneyAccount);
                } 
                if (trade.MoneyAccountId != 0)
                {
                    var oldMoney = oldTrade != null && oldTrade.MoneyAccountId == trade.MoneyAccountId 
                        ? oldTrade.Value * (oldTrade.IsReceived ? 1 : -1) 
                        : 0;
                    var money = (trade.Value * (trade.IsReceived ? 1 : -1))
                         - oldMoney;
                    var newMoneyAccount = await _accountRepository.GetById(trade.MoneyAccountId, trade.UserId);
                    newMoneyAccount.Total += money;
                    await _accountRepository.SaveAccount(newMoneyAccount);
                }
            }
            return await _repository.SaveTrade(trade);
        }
        var createdAt = trade.CreatedAt;
        if (trade.DebtId > 0)
        {
            var item = await _debtRepository.GetById(trade.DebtId, trade.UserId);
            createdAt = item.CreatedAt.Value;
        }
        else if (trade.OrderId > 0)
        {
            var item = await _orderRepository.GetById(trade.OrderId, trade.UserId);
            createdAt = item.CreatedAt.HasValue ? item.CreatedAt.Value : DateTime.Now;
        }
        else if (trade.ReceivedNoteId > 0)
        {
            var item = await _receivedNoteRepository.GetById(trade.ReceivedNoteId, trade.UserId);
            createdAt = item.CreatedAt.HasValue ? item.CreatedAt.Value : DateTime.Now;
        }
        trade.CreatedAt = createdAt;
        if (trade.MoneyAccountId != 0 && trade.SaveAccount.HasValue && trade.SaveAccount.Value)
        {
            var moneyAccount = await _accountRepository.GetById(trade.MoneyAccountId, trade.UserId);
            moneyAccount.Total += trade.Value * (trade.IsReceived ? 1 : -1);
            await _accountRepository.SaveAccount(moneyAccount);
        }
        return await _repository.SaveTrade(trade);
    }
}