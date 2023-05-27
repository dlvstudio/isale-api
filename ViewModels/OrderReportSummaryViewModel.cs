using System;
using System.Collections.Generic;

public class OrderReportSummaryViewModel {
    public decimal Total { get; set; } = 0;
    public decimal TotalDiscount { get; set; } = 0;
    public decimal TotalShip { get; set; } = 0;
    public decimal TotalNoVAT { get; set; } = 0;
    public decimal TotalVAT { get; set; } = 0;
    public decimal TotalPaid { get; set; } = 0;
    public decimal TotalChange { get; set; } = 0;
    public decimal TotalCost { get; set; } = 0;
    public decimal TotalRevenue { get; set; } = 0;
    public decimal TotalStaffRevenue { get; set; } = 0;
    public decimal TotalQuantity { get; set; } = 0;
    public decimal TotalProducts { get; set; } = 0;
    public DateTime FromDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalItems { get; set; } = 0;
    public int Type { get; set; } = 0;
    public List<OrderReportItemViewModel> Items { get; set;}
}