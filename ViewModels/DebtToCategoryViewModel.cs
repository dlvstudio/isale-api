using System.Collections.Generic;
using System.Linq;

public class DebtToCategoryViewModel {
    public int Id { get; set; }
    public int DebtId { get; set; }
    public int CategoryId { get; set; }
    public string UserId { get; set; }
    public static IEnumerable<DebtToCategory> ConvertToModel(IEnumerable<DebtToCategoryViewModel> input) {
        return input.Select(t => new DebtToCategory() {
            Id = t.Id,
            DebtId = t.DebtId,
            CategoryId = t.CategoryId,
            UserId = t.UserId
        })
        .ToList();
    }
}