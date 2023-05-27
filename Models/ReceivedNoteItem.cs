using System;

public class ReceivedNoteItem {
    public int Id {get;set;}
    public int ReceiptId {get;set;}
    public int ProductId {get;set;}
    public string Note {get;set;}
    public string ProductCode {get;set;}
    public string ProductName {get;set;}
    public decimal? Quantity {get;set;}
    public string Unit {get;set;}
    public decimal UnitPrice {get;set;}
    public decimal? CostPrice {get;set;}
    public decimal? UnitPriceForeign {get;set;}
    public string ForeignCurrency {get;set;}
    public decimal Amount {get;set;}
    public decimal? AmountForeign {get;set;}
    public bool IsExpand {get;set;}
    public decimal Discount {get;set;}
    //0: %, 1: manual
    public int DiscountType {get;set;}
    public string Barcode {get;set;}
    public DateTime? ReceivedDate {get;set;}
    public decimal? UnitExchange {get;set;}
    public string BasicUnit {get;set;}
}