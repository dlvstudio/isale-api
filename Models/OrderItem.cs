
using System.Collections.Generic;

public class OrderItem {
    public int Id {get;set;}
    public int ProductId {get;set;}
    public string ProductCode {get;set;}
    public string ProductName {get;set;}
    public string ProductAvatar {get;set;}
    public decimal? Price {get;set;}
    public decimal? CostPrice {get;set;}
    public string Unit {get;set;}
    public decimal? Count {get;set;}
    public decimal? Total {get;set;}
    public decimal? TotalCostPrice {get;set;}
    public decimal? Discount {get;set;}
    public int DiscountType {get;set;}
    public bool IsExpand {get;set;}
    public bool? IsCombo {get;set;}
    public IEnumerable<OrderSubItem> Items {get;set;}
    public IEnumerable<OrderSubItem> Materials {get;set;}
    public IEnumerable<OrderSubItem> Options {get;set;}
    public string UserId {get;set;}
    public string Note {get;set;}
    public decimal? UnitExchange {get;set;}
    public string BasicUnit {get;set;}
    public decimal? ShopPrice {get;set;}
    public PriceInfo PriceInfo { get;set; }
    public IEnumerable<TradeToCategory> Categories {get;set;}
}