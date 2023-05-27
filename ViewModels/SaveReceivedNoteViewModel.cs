using System;

public class SaveReceivedNoteViewModel {
    public int Id {get;set;}
    public DateTime? CreatedAt {get;set;}
    public int? ContactId {get;set;}
    public int? StaffId {get;set;}
    public int? MoneyAccountId {get;set;}
    public string ContactName {get;set;}
    public string ContactPhone {get;set;}
    public decimal? ShippingFee {get;set;}
    public int? TaxType {get;set;}
    public decimal? Tax {get;set;}
    public decimal? NetValue {get;set;}
    public decimal? Discount {get;set;}
    public decimal? DiscountOnTotal {get;set;}
    public decimal? Total {get;set;}
    public decimal? Paid {get;set;}
    public decimal? TotalForeign {get;set;}
    public int? Status {get;set;}
    public string ItemsJson {get;set;}
    public string DeliveryPerson {get;set;}
    public string Receiver {get;set;}
    public string ForeignCurrency {get;set;}
    public int? StoreId {get;set;}
    public bool? SaveProductNotes {get;set;}
    public string Lang {get;set;}
}