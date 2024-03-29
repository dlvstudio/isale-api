using System;

public class SaveOrderViewModel {
    public int Id {get;set;}
    public DateTime? CreatedAt {get;set;}
    public string OrderCode {get;set;}
    public int? ContactId {get;set;}
    public int? StaffId {get;set;}
    public int? CollaboratorId {get;set;}
    public int? MoneyAccountId {get;set;}
    public string ContactName {get;set;}
    public string ContactPhone {get;set;}
    public string ContactAddress {get;set;}
    public decimal? ShippingFee {get;set;}
    public int? TaxType {get;set;}
    public decimal? Tax {get;set;}
    public decimal? NetValue {get;set;}
    public decimal? Discount {get;set;}
    public decimal? DiscountOnTotal {get;set;}
    public decimal? Total {get;set;}
    public decimal? Paid {get;set;}
    public decimal? Change {get;set;}
    public int? Status {get;set;}
    public string ItemsJson {get;set;}
    public int? TableId {get;set;}
    public string Note {get;set;}
    public int? StoreId {get;set;}
    public string BillOfLadingCode {get;set;}
    public string ShippingPartner {get;set;}
    public string ShipperName {get;set;}
    public string ShipperPhone {get;set;}
    public string DeliveryAddress {get;set;}
    public int? ShipperId {get;set;}
    public bool? HasShipInfo {get;set;}
    public bool? SaveProductNotes {get;set;}
    public string Lang {get;set;}
    public decimal? PointAmount {get;set;}
    public decimal? PointPaymentExchange {get;set;}
    public decimal? AmountFromPoint {get;set;}
    public bool? ShipCostOnCustomer {get;set;}
    public decimal? TotalPromotionDiscount {get;set;}
    public string PromotionsJson {get;set;}
}