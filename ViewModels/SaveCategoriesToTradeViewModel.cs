using System.Collections.Generic;

public class SaveCategoriesToTradeViewModel {
    public IEnumerable<TradeToCategoryViewModel> Categories { get; set; }
    public int? TradeId { get; set; }
    public int? StaffId { get; set; }
}