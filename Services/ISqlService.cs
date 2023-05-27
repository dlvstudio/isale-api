using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISqlService
{
    Task<IEnumerable<ColumnType>> Columns(string table);
    Task<IEnumerable<string>> Tables();

    Task<int> SaveAsync(Dictionary<string, object> model, string table, string primaryKey, List<string> whereUpdateFields, Dictionary<string, string> exceptionFunctions);
    
    Task<bool> RemoveAsync(string table, Dictionary<string, object> model);

    Task<Dictionary<string, object>> GetByIdAsync(string table, int id, string userId);

    Task<Dictionary<string, object>> GetAsync(string table, Dictionary<string, object> model, QueryModelOnSearch queryModelOnSearch);

    Task<bool> UpdateAsync(string table, Dictionary<string, object> model, QueryModelOnSearch queryModelOnSearch);

    Task<IEnumerable<Dictionary<string, object>>> ListAsync(string table, Dictionary<string, object> model, QueryModelOnSearch queryModelOnSearch);
    
    Task<int> CountAsync(string table, Dictionary<string, object> model, QueryModelOnSearch queryModelOnSearch);
}
