using System;
using System.Collections.Generic;

public class ProductReportViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalCost { get; set; } = 0;
    public decimal TotalSale { get; set; } = 0;
    public int TotalItems { get; set; } = 0;
    public List<ProductReportItemViewModel> Items { get; set;}
}