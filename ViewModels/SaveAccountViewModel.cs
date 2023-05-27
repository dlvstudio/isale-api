using System;

public class SaveAccountViewModel {
    public int Id { get; set; }
    public string UserId { get; set; }
    public string AccountName { get; set; }
    public decimal? Total { get; set; }
    public string CreatedAt { get; set; }
    public string ModifiedAt { get; set; }
    public bool? IsDefault { get; set; }
    public string BankAccountName { get; set; }
    public string BankName { get; set; }
    public string BankNumber { get; set; }
}