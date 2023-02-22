public class SaveProductNoteViewModel {
    public int Id {get;set;}
    public int? ReceivedNoteId {get;set;}
    public int? TransferNoteId {get;set;}
    public int? StaffId {get;set;}
    public int? OrderId {get;set;}
    public int? TradeId {get;set;}
    public int? ProductId {get;set;}
    public int? ContactId {get;set;}
    public string ProductCode {get;set;}
    public string ProductName {get;set;}
    public string Note {get;set;}
    public decimal? Quantity {get;set;}
    public string Unit {get;set;}
    public decimal? UnitPrice {get;set;}
    public decimal? UnitPriceForeign {get;set;}
    public string ForeignCurrency {get;set;}
    public decimal Amount {get;set;}
    public decimal? AmountForeign {get;set;}
    public decimal Discount {get;set;}
    //0: %, 1: manual
    public int DiscountType {get;set;}
    public string CreatedAt { get; set; }
    public int? StoreId {get;set;}
    public decimal? UnitExchange { get;set; }
    public string BasicUnit { get;set; }
}