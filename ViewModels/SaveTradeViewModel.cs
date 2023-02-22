using System;

public class SaveTradeViewModel {
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int? StaffId { get; set; }
    public int ProductId { get; set; }
    public int? ReceivedNoteId { get; set; }
    public int? OrderId { get; set; }
    public int? MoneyAccountId { get; set; }
    public int? TradeId { get; set; }
    public int? DebtId { get; set; }
    public bool IsReceived { get; set; }
    public decimal Value { get; set; }
    public string Note { get; set; }
    
    public bool? IsPurchase { get; set; }
    public int? ProductCount { get; set; }
    public string ImageUrlsJson { get; set; }
    public string AvatarUrl { get; set; }
    public string CreatedAt { get; set; }
    public string ModifiedAt { get; set; }
    public bool? SaveAccount { get; set; }
}