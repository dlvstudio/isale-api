using System;
using System.Collections.Generic;

public class ReceivedNote {
    public int Id {get;set;}
    public string UserId {get;set;}
    public string ContactName {get;set;}
    public string ContactPhone {get;set;}
    public int ContactId {get;set;}
    public Contact Contact {get;set;}
    public int StaffId {get;set;}
    public Staff Staff {get;set;}
    public decimal Total {get;set;}
    public decimal? Paid {get;set;}
    public decimal? TotalForeign {get;set;}
    public DateTime? CreatedAt {get;set;}
    public string DeliveryPerson {get;set;}
    public string Receiver {get;set;}
    public IEnumerable<ReceivedNoteItem> Items {get;set;}
    public string ItemsJson {get;set;}
    public int MoneyAccountId {get;set;}
    public Account MoneyAccount {get;set;}
    public int TaxType {get;set;}
    public decimal Tax {get;set;}
    public decimal NetValue {get;set;}
    public decimal Discount {get;set;}
    public decimal DiscountOnTotal {get;set;}
    public decimal ShippingFee {get;set;}
    public string ForeignCurrency {get;set;}
    public int StoreId { get; set; }
}