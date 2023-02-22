using System;

public class SaveDebtViewModel {
    public int Id { get; set; }
    public int ContactId { get; set; }
    public int ProductId { get; set; }
    public int? OrderId { get; set; }
    public int? ReceivedNoteId { get; set; }
    public int DebtType { get; set; }
    public decimal Value { get; set; }
    public decimal? ValuePaid { get; set; }
    public int? CountPaid { get; set; }
    public string Note { get; set; }
    
    public bool? IsPurchase { get; set; }
    public int? ProductCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string ModifiedAt { get; set; }

    public DateTime? MaturityDate { get; set; }
    public bool? IsPaid { get; set; }
    public int? InterestRate { get; set; }
    public int? StaffId {get;set;}
    public int? StoreId {get;set;}
}