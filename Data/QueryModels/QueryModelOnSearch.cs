using System.Collections.Generic;
using Dapper;

public class QueryModelOnSearch
{
    public Dictionary<string, List<string>> WhereFieldQuerys { get; set; }
    public Dictionary<string, List<string>> UpdateFieldQuerys { get; set; }
    public List<string> OnlySelectFields { get; set; }
    public List<string> ExcludeFields { get; set; }
    public int MaxRecordCount { get; set; }
    public static QueryModelOnSearch CreateEqualsFromModel(Dictionary<string, object> model)
    {
        var queryModel = new QueryModelOnSearch();
        queryModel.WhereFieldQuerys = new Dictionary<string, List<string>>();
        var keys = model.Keys.AsList();
        foreach (var key in keys)
        {
            queryModel.WhereFieldQuerys[key.ToLower()] = new List<string>() { EnumSearchFunctions.EQUALS };
        }
        return queryModel;
    }
}