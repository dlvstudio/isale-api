using System;

public class Debt {
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int ProductId { get; set; }
    public int OrderId { get; set; }
    public int StaffId {get;set;}
    public int ReceivedNoteId { get; set; }
    public int CountPaid { get; set; }
    public int DebtType { get; set; }
    public decimal Value { get; set; }
    public decimal ValuePaid { get; set; }
    public string Note { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public DateTime? MaturityDate { get; set; }
    public bool IsPurchase { get; set; }
    public bool IsPaid { get; set; }
    public int InterestRate { get; set; }
    public int ProductCount { get; set; }
    public string UserId { get; set; }
    public Contact Contact { get; set; }
    public Product Product { get; set; }
    public Order Order { get; set; }
    public Staff Staff { get; set; }
    public ReceivedNote ReceivedNote { get; set; }
    public int StoreId { get; set; }
}