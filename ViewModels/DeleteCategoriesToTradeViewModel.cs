using System.Collections.Generic;

public class DeleteCategoriesToTradeViewModel {
    public IEnumerable<TradeToCategoryViewModel> Categories { get; set; }
    public int? StaffId {get;set;}
}