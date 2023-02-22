using System;
using System.Collections.Generic;

public class ProductNoteReportViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Total { get; set; } = 0;
    public int TotalItems { get; set; } = 0;
    public List<ProductNoteReportItemViewModel> Items { get; set;}
}