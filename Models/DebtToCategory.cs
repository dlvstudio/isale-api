public class DebtToCategory {
    public int Id { get; set; }
    public int DebtId { get; set; }
    public int CategoryId { get; set; }
    public string UserId { get; set; }
    public Category TradeCategory { get; set; }
    public Debt Debt { get; set; }
}