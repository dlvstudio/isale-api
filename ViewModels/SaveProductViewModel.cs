using System;

public class SaveProductViewModel {
    public int Id { get; set; }
    public string Code { get; set; }
    public string Barcode { get; set; }
    public decimal? Count { get; set; }
    public decimal? Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? CostPriceForeign { get; set; }
    public string ForeignCurrency { get; set; }
    public bool IsSale { get; set; }
    public string ImageUrlsJson { get; set; }
    public string AvatarUrl { get; set; }
    public string Unit { get; set; }
    public string UnitsJson { get; set; }
    public string Title { get; set; }
    public bool IsOption { get; set; }
    public bool IsCombo { get; set; }
    public bool IsPublic { get; set; }
    public int? Status { get; set; }
    public bool IsService { get; set; }
    public string ItemsJson {get;set;}
    public string MaterialsJson {get;set;}
    public bool? ShowOnWeb { get; set; }
    public bool? IsHotProduct { get; set; }
    public bool? IsNewProduct { get; set; }
    public bool? ShowPriceOnWeb { get; set; }
    public string Description {get;set; }
    public DateTime? ExpiredAt {get;set; }
    public int? StaffId {get;set; }
    public bool? IsMaterial { get; set; }
}