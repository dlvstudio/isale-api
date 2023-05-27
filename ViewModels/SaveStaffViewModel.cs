public class SaveStaffViewModel {
    public int Id { get; set; }
    public string AvatarUrl { get; set; }
    public string UserName { get; set; }
    public string ShopName { get; set; }
    public string Name { get; set; }
    public bool? HasFullAccess { get; set; }
    public bool? CanCreateOrder { get; set; }
    public bool? CanUpdateDeleteOrder { get; set; }
    public bool? CanCreateNewTransaction { get; set; }
    public bool? CanUpdateDeleteTransaction { get; set; }
    public bool? CanCreateUpdateDebt { get; set; }
    public bool? CanCreateUpdateNote { get; set; }
    public bool? CanUpdateDeleteProduct { get; set; }
    public bool? CanViewProductCostPrice { get; set; }
    public bool? CanUpdateProductCostPrice { get; set; }
    public bool? CanViewAllContacts { get; set; }
    public bool? CanManageContacts { get; set; }
    public bool? UpdateStatusExceptDone { get; set; }
    public int? HourLimit { get; set; }
    public int? StoreId {get;set;}
    public int? ShiftId {get;set;}
    public bool? BlockViewingQuantity { get; set; }
    public bool? BlockEditingOrderPrice { get; set; }
}