using System.Collections.Generic;
using System.Linq;

public class TradeToCategoryViewModel {
    public int Id { get; set; }
    public int TradeId { get; set; }
    public int CategoryId { get; set; }
    public string UserId { get; set; }
    public static IEnumerable<TradeToCategory> ConvertToModel(IEnumerable<TradeToCategoryViewModel> input) {
        return input.Select(t => new TradeToCategory() {
            Id = t.Id,
            TradeId = t.TradeId,
            CategoryId = t.CategoryId,
            UserId = t.UserId
        })
        .ToList();
    }
}