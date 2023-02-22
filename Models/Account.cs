using System;

public class Account {
    public int Id { get; set; }
    public string UserId { get; set; }
    public string AccountName { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDefault { get; set; }
    public string BankAccountName { get; set; }
    public string BankName { get; set; }
    public string BankNumber { get; set; }
    public int DefaultStoreId { get; set; }
}