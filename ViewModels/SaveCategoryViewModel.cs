public class SaveCategoryViewModel {
    public int Id { get; set; }
    public string Code { get; set; }
    public string Barcode { get; set; }
    public int? Count { get; set; }
    public int? OrderIndex { get; set; }
    public decimal? Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public bool IsSale { get; set; }
    public string ImageUrlsJson { get; set; }
    public string AvatarUrl { get; set; }
    public string Unit { get; set; }
    public string Title { get; set; }
    public int? StaffId { get; set; }
}