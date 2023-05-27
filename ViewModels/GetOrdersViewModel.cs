using System.Collections.Generic;
public class GetOrdersViewModel {
    public IEnumerable<int> OrderIds { get; set; }
    public string DateFrom { get; set; }
    public string DateTo { get; set; }
    public int? ContactId { get; set; }
    public int? Status { get; set; }
    public int? StaffId { get; set; }
    public int? ReportType { get; set; }
    public int? ProductId { get; set; }
    public string Lang { get; set; }
    public int? StoreId { get; set; }
    public bool? IsMaterial { get; set; }
    public bool? AutoUpdateQuantity { get; set; }
}