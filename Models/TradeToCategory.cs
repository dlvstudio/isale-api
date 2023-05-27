public class TradeToCategory {
    public int Id { get; set; }
    public int TradeId { get; set; }
    public int CategoryId { get; set; }
    public string UserId { get; set; }
    public Category TradeCategory { get; set; }
    public Trade Trade { get; set; }
}