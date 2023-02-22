using System;
using System.Collections.Generic;

public class OrderReportItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string Unit { get; set; }
    public decimal Discount { get; set; }
    public decimal NetValue { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal? Paid { get; set; }
    public decimal? Change { get; set; }
    public string Contact { get; set; }
    public string ContactAddress { get; set; }
    public int Status { get; set; }
    public decimal Total { get; set; }
    public decimal Cost { get; set; }
    public decimal Revenue { get; set; }
    public decimal StaffRevenue { get; set; }
    public string Staff { get; set; }
    public string BillOfLadingCode {get;set;}
    public string ShippingPartner {get;set;}
    public string ShipperName {get;set;}
    public string ShipperPhone {get;set;}
    public bool ShipCostOnCustomer {get;set;}
    public string DeliveryAddress {get;set;}
    public decimal Quantity { get; set; }
    public IEnumerable<OrderItem> Items { get; set; }
    public List<OrderReportItemViewModel> SubItems { get; set; }
}