using System;

public class ProductNote {
    public int Id {get;set;}
    public int ReceivedNoteId {get;set;}
    public int OrderId {get;set;}
    public int TradeId {get;set;}
    public int TransferNoteId {get;set;}
    public int ProductId {get;set;}
    public int ContactId {get;set;}
    public Order Order {get;set;}
    public ReceivedNote ReceivedNote {get;set;}
    public Product Product {get;set;}
    public Contact Contact {get;set;}
    public Trade Trade {get;set;}
    public TransferNote TransferNote {get;set;}
    public string ProductCode {get;set;}
    public string ProductName {get;set;}
    public string Note {get;set;}
    public decimal Quantity {get;set;}
    public string Unit {get;set;}
    public decimal UnitPrice {get;set;}
    public decimal? UnitPriceForeign {get;set;}
    public string ForeignCurrency {get;set;}
    public decimal Amount {get;set;}
    public decimal? AmountForeign {get;set;}
    public decimal Discount {get;set;}
    //0: %, 1: manual
    public int DiscountType {get;set;}
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string UserId { get; set; }
    public DateTime? ReceivedDate {get;set;}
    public int StoreId {get;set;}
    public decimal? UnitExchange { get;set; }
    public string BasicUnit { get;set; }
}