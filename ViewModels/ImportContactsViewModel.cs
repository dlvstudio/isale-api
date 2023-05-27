using System.Collections.Generic;

public class ImportContactsViewModel {
    public string Error { get; set; }
    public int Count { get; set; }
    // keep this for fucking old versions
    public int Id { get; set; }
    public IEnumerable<Contact> Contacts { get; set; }
}