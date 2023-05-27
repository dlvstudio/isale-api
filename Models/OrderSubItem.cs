
using System.Collections.Generic;

public class OrderSubItem {
    public int Id {get;set;}
    public decimal Count {get;set;}
    public string MaterialsJson {get;set;}
    public IEnumerable<OrderSubItem> Materials {get;set;}
    public IEnumerable<OrderSubItem> Options {get;set;}
    public decimal? Price {get;set;}
    public string Unit {get;set;}
    public string BasicUnit {get;set;}
    public decimal? UnitExchange {get;set;}
    public int ProductId {get;set;}
    public string ProductCode {get;set;}
    public string Code {get;set;}
    public string ProductName {get;set;}
    public string Title {get;set;}
    public string ProductAvatar {get;set;}
}