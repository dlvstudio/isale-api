using System;

public class SaveTransferNoteViewModel {
    public int Id {get;set;}
    public string Code {get;set;}
    public DateTime? CreatedAt {get;set;}
    public int? StaffId {get;set;}
    public string Deliverer {get;set;}
    public string Transportation {get;set;}
    public string Receiver {get;set;}
    public string DeliveryAddress {get;set;}
    public string ItemsJson {get;set;}
    public DateTime? ModifiedAt {get;set;}
    public int? ExportStoreId {get;set;}
    public int? ImportStoreId {get;set;}
    public int? ImportMoneyAccountId {get;set;}
    public int? ExportMoneyAccountId {get;set;}
    public string UserId {get;set;}
    public bool? HasPayment {get;set;}
}