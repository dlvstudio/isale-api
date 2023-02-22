public class SaveAccountItemViewModel {
    public int Id { get; set; }
    public string UserId { get; set; }
    public int? TradeId { get; set; }
    public int? OrderId { get; set; }
    public int MoneyAccountId { get; set; }
    public string Note { get; set; }
    public decimal? Value { get; set; }
    public decimal? TransferFee { get; set; }
}