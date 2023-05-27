using System.Collections.Generic;

public class SaveCategoriesToDebtViewModel {
    public IEnumerable<DebtToCategoryViewModel> Categories { get; set; }
    public int? TradeId { get; set; }
    public int? StaffId { get; set; }
}