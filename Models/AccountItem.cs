using System;

public class AccountItem {
    public int Id { get; set; }
    public int TradeId { get; set; }
    public int OrderId { get; set; }
    public int MoneyAccountId { get; set; }
    public string Note { get; set; }
    public decimal Value { get; set; }
    public decimal TransferFee { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; }
    public Account MoneyAccount { get; set; }
}