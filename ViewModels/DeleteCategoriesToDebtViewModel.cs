using System.Collections.Generic;

public class DeleteCategoriesToDebtViewModel {
    public IEnumerable<DebtToCategoryViewModel> Categories { get; set; }
    public int? StaffId {get;set;}
}