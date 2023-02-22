using System;

public class Trade {
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int StaffId { get; set; }
    public int MoneyAccountId { get; set; }
    public int ReceivedNoteId { get; set; }
    public int TransferNoteId { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int DebtId { get; set; }
    public bool IsReceived { get; set; }
    public decimal Value { get; set; }
    public decimal Fee { get; set; }
    public string Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsPurchase { get; set; }
    public int ProductCount { get; set; }
    public string ImageUrlsJson { get; set; }
    public string AvatarUrl { get; set; }
    public string UserId { get; set; }
    public Contact Contact { get; set; }
    public Product Product { get; set; }
    public Staff Staff { get; set; }
    public Order Order { get; set; }
    public Account MoneyAccount { get; set; }
    public Debt Debt { get; set; }
    public ReceivedNote ReceivedNote { get; set; }
    public bool? SaveAccount { get; set; }
}