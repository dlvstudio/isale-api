using System.Collections.Generic;

public class ImportProductsViewModel {
    public string Error { get; set; }
    public int Count { get; set; }
    // keep this for fucking old versions
    public int Id { get; set; }
    public IEnumerable<Product> Products { get; set; }
}