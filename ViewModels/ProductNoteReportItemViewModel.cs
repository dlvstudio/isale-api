using System;

public class ProductNoteReportItemViewModel
{
    
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; } = 0;
    public decimal Discount { get; set; } = 0;
    public string Description { get; set; }
    public decimal? UnitPriceForeign { get; set; }
    public decimal? AmountForeign { get; set; }
    public string Contact { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal Amount { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public string Staff { get; set; }
    public decimal StaffUnitPrice { get; set; }
    public decimal StaffProfit { get; set; }
}