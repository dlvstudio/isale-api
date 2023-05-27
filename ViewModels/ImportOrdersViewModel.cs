using System.Collections.Generic;

public class ImportOrdersViewModel {
    public string Error { get; set; }
    public int Count { get; set; }
    public IEnumerable<Order> Orders { get; set; }
}