using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class SqlService : ISqlService
{
    private const string DBNAME = "isale";
    private const string PREFIX_TABLE = "";
    private readonly IConfiguration _config;

    private Dictionary<string, Dictionary<string, string>> tableMap;

    public SqlService(
        IConfiguration config
    ) {
        _config = config;
        tableMap = BuildMapHelper.BuildMap();
    }

    public async Task<IEnumerable<ColumnType>> Columns(string table) {
        using (var db = AppDb)
        {
            string query = string.Format( 
                @"SELECT `COLUMN_NAME` as `Name`, `DATA_TYPE` as `Type` 
                FROM `INFORMATION_SCHEMA`.`COLUMNS` 
                WHERE `TABLE_SCHEMA`='{0}' 
                    AND `TABLE_NAME`='{1}{2}'", DBNAME, PREFIX_TABLE, table);
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<ColumnType>(query);
            return result;
        }
    }

    public async Task<IEnumerable<string>> Tables() {
        using (var db = AppDb)
        {
            string query = string.Format( 
                @"SELECT table_name FROM information_schema.tables
                WHERE table_schema = '{0}';", DBNAME);
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryAsync<string>(query);
            return result;
        }
    }

    public async Task<int> SaveAsync(Dictionary<string, object> model, string table, string primaryKey, List<string> whereUpdateFields, Dictionary<string, string> exceptionFunctionsInput) {
        var updateFields = model.Keys.AsList();
        if (!updateFields.Any()) {
            return 0;
        }
        var primaryKeyInDic = updateFields.FirstOrDefault(f => f.ToLower() == primaryKey.ToLower());
        var hasPrimaryKeyField = !string.IsNullOrWhiteSpace(primaryKeyInDic);
        var exceptionFunctions = LowerKeysOfModel(exceptionFunctionsInput);
        var primaryKeyValue = 0;
        var canParsePrimaryKey = hasPrimaryKeyField
            && int.TryParse(model[primaryKeyInDic].ToString(), out primaryKeyValue);
        if (hasPrimaryKeyField && !canParsePrimaryKey) {
            return 0;
        }
        var tableNameUpper = table.ToUpper();
        if (!tableMap.ContainsKey(tableNameUpper)) {
            return 0;
        }
        string query = string.Empty;
        var isInsert = primaryKeyValue == 0;
        if (isInsert) {
            string queryInsertTemplate = 
            @"INSERT INTO `{0}`
                ({1})
            VALUES
                ({2});
            SELECT LAST_INSERT_ID();";
            List<string> queryInsertFieldArrs = new List<string>();
            List<string> queryInsertValueArrs = new List<string>();
            foreach (var updateField in updateFields)
            {
                var updateFieldLower = updateField.ToLower();
                if (updateFieldLower != primaryKey.ToLower() && tableMap[tableNameUpper].ContainsKey(updateFieldLower)) {
                    queryInsertFieldArrs.Add(string.Format("`{0}`", updateFieldLower));
                    queryInsertValueArrs.Add(string.Format("@{0}", updateFieldLower));
                }
            }
            query = string.Format(queryInsertTemplate, 
                tableNameUpper,
                string.Join("\n,", queryInsertFieldArrs),
                string.Join("\n,", queryInsertValueArrs));
        } else {
            string queryUpdateTemplate = 
            @"UPDATE `{0}`
            SET
                {1}
            WHERE
                {2};";
            
            List<string> queryUpdateFieldArrs = new List<string>();
            List<string> queryWhereFieldArrs = new List<string>();
            foreach (var updateField in updateFields)
            {
                var updateFieldLower = updateField.ToLower();
                var isWhereField = whereUpdateFields.Any(f => f.ToLower() == updateFieldLower);
                if (!isWhereField && updateFieldLower != primaryKey.ToLower() && tableMap[tableNameUpper].ContainsKey(updateFieldLower)) {
                    queryUpdateFieldArrs.Add(string.Format("`{0}` = @{0}", updateFieldLower));
                } else if (isWhereField) {
                    queryWhereFieldArrs.Add(string.Format("`{0}` = @{0}", updateFieldLower));
                }
            }
            query = string.Format(queryUpdateTemplate, 
                tableNameUpper,
                string.Join("\n,", queryUpdateFieldArrs),
                string.Join("\n AND ", queryWhereFieldArrs));
        }
        using (var db = AppDb)
        {   
            await db.Connection.OpenAsync();
            var dataModel = BuildDataModel(table, model, exceptionFunctions);
            if (isInsert) {
                var insertResult = (await db.Connection.QueryAsync<int>(query, dataModel)).Single();
                return insertResult;
            }
            var postResult = await db.Connection.ExecuteAsync(query, dataModel);
            return postResult > 0 ? primaryKeyValue : 0; 
        }
    }

    private Dictionary<string, object> BuildDataModel(string table, Dictionary<string, object> inputModel, Dictionary<string, string> exceptionFunctions) {
        if (inputModel == null) {
            return null;
        }
        var model = LowerKeysOfModel(inputModel);
        var updateFields = model.Keys.AsList();
        if (!updateFields.Any()) {
            return null;
        }
        var tableNameUpper = table.ToUpper();
        var preProcessModel = new Dictionary<string, object>();
        foreach (var updateField in updateFields)
        {
            if (!tableMap[tableNameUpper].ContainsKey(updateField)) {
                continue;
            } 
            preProcessModel[updateField] = ProcessFieldValue(table, updateField, model[updateField], tableMap[tableNameUpper][updateField], exceptionFunctions);

        }
        return preProcessModel;
    }

    private object ProcessFieldValue(string table, string fieldName, object fieldValue, string dataType, Dictionary<string, string> exceptionFunctions) {
        var tableNameUpper = table.ToUpper();
        var isEmpty = fieldValue == null 
                || fieldValue != null && string.IsNullOrWhiteSpace(fieldValue.ToString());
        switch(dataType) {
            case "timestamp":
                var dateValue = !isEmpty ? fieldValue : null;
                if (exceptionFunctions != null && exceptionFunctions.ContainsKey(fieldName)) {
                    var func = exceptionFunctions[fieldName];
                    if (func == EnumExceptionFunctions.EMPTY_THEN_NOW && isEmpty) {
                        return DateTime.Now;
                    }
                }
                return dateValue;
            case "int":
                var numberValue = !isEmpty ? fieldValue : null;
                if (exceptionFunctions != null && exceptionFunctions.ContainsKey(fieldName)) {
                    var func = exceptionFunctions[fieldName];
                    if (func == EnumExceptionFunctions.EMPTY_THEN_ZERO && isEmpty) {
                        return 0;
                    }
                }
                return numberValue;
            case "bit":
                var bitValue = !isEmpty ? fieldValue : null;
                if (exceptionFunctions != null && exceptionFunctions.ContainsKey(fieldName)) {
                    var func = exceptionFunctions[fieldName];
                    if (func == EnumExceptionFunctions.EMPTY_THEN_FALSE && isEmpty) {
                        return false;
                    }
                }
                return bitValue;
            case "decimal":
                var decimalValue = !isEmpty ? fieldValue : null;
                if (exceptionFunctions != null && exceptionFunctions.ContainsKey(fieldName)) {
                    var func = exceptionFunctions[fieldName];
                    if (func == EnumExceptionFunctions.EMPTY_THEN_ZERO && isEmpty) {
                        return 0;
                    }
                }
                return decimalValue;
            default:
                return fieldValue;
        }
    }

    public AppDb AppDb
    {
        get
        {
            return new AppDb(_config.GetConnectionString("DefaultConnection"));
        }
    }

    public async Task<bool> UpdateAsync(string table, Dictionary<string, object> model, QueryModelOnSearch queryModelOnSearch)
    {
        var tableNameUpper = table.ToUpper();
        if (!tableMap.ContainsKey(tableNameUpper)) {
            return false;
        }
        string query = string.Empty;
        string queryTemplate = 
            @"UPDATE `{0}`
            SET
                {1}
            WHERE
                {2};";
        var queryModel = queryModelOnSearch;
        if (queryModel == null) {
            queryModel = new QueryModelOnSearch();
            queryModel.WhereFieldQuerys = new Dictionary<string, List<string>>();
            var keys = model.Keys.AsList();
            foreach (var key in keys)
            {
                queryModel.WhereFieldQuerys[key.ToLower()] = new List<string>() {EnumSearchFunctions.EQUALS};
            }
        }

        var updateFieldQuerys = LowerKeysOfModel(queryModel.UpdateFieldQuerys);
        var updateFields = updateFieldQuerys.Keys.AsList();
        List<string> queryUpdateFieldArrs = new List<string>();
        foreach (var updateField in updateFields)
        {
            var fieldLower = updateField.ToLower();
            if (tableMap[tableNameUpper].ContainsKey(fieldLower)) {
                if (updateFieldQuerys.ContainsKey(fieldLower) && updateFieldQuerys[fieldLower].Any()) {
                    var oper = updateFieldQuerys[fieldLower][0];
                    if (oper== EnumUpdateFunctions.EQUALS) {
                        var paramName = updateFieldQuerys[fieldLower].Count() > 1 
                            ? updateFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryUpdateFieldArrs.Add(string.Format("`{0}` = @{1}", fieldLower, paramName));
                    } else if (oper== EnumUpdateFunctions.INCREASE) {
                        var paramName = updateFieldQuerys[fieldLower].Count() > 1 
                            ? updateFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryUpdateFieldArrs.Add(string.Format("`{0}` = `{0}` + @{1}", fieldLower, paramName));
                    } else if (oper== EnumUpdateFunctions.DECREASE) {
                        var paramName = updateFieldQuerys[fieldLower].Count() > 1 
                            ? updateFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryUpdateFieldArrs.Add(string.Format("`{0}` = `{0}` - @{1}", fieldLower, paramName));
                    } 
                }
            }
        }

        var whereFieldQuerys = LowerKeysOfModel(queryModel.WhereFieldQuerys);
        var whereFields = whereFieldQuerys.Keys.AsList();
        List<string> queryWhereFieldArrs = new List<string>();
        foreach (var whereField in whereFields)
        {
            var fieldLower = whereField.ToLower();
            if (tableMap[tableNameUpper].ContainsKey(fieldLower)) {
                if (whereFieldQuerys.ContainsKey(fieldLower) && whereFieldQuerys[fieldLower].Any()) {
                    var oper = whereFieldQuerys[fieldLower][0];
                    if (oper== EnumSearchFunctions.EQUALS) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` = @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.BETWEENS) {
                        queryWhereFieldArrs.Add(string.Format("`{0}` >= @{1} AND `{0}` <= @{2}", fieldLower, whereFieldQuerys[fieldLower][1].ToLower(), whereFieldQuerys[fieldLower][2].ToLower()));
                    } else if (oper== EnumSearchFunctions.ZERO_THEN_NOT_FILTER_BY_THIS) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("(@{1} = 0 OR `{0}` >= @{1})", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.IN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` IN @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.BIGGER_THAN_TODAY) {
                        queryWhereFieldArrs.Add(string.Format("DATE(`{0}`) >= DATE(NOW())", fieldLower));
                    } else if (oper== EnumSearchFunctions.SMALLER_THAN_TODAY) {
                        queryWhereFieldArrs.Add(string.Format("DATE(`{0}`) <= DATE(NOW())", fieldLower));
                    } else if (oper== EnumSearchFunctions.IS_TODAY) {
                        queryWhereFieldArrs.Add(string.Format("DATE(`{0}`) = DATE(NOW())", fieldLower));
                    } else if (oper== EnumSearchFunctions.SMALLER_THAN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` < @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.SMALLER_OR_EQUALS_THAN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` <= @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.BIGGER_THAN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` > @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.BIGGER_OR_EQUALS_THAN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` >= @{1}", fieldLower, paramName));
                    }
                }
            }
        }
        query = string.Format(queryTemplate, 
            tableNameUpper,
            string.Join("\r\n, ", queryUpdateFieldArrs),
            string.Join("\r\n AND ", queryWhereFieldArrs)
        );
        model = LowerKeysOfModel(model);
        using (var db = AppDb)
        {   
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, model);
            return postResult > 0;
        }
    }

    public async Task<bool> RemoveAsync(string table, Dictionary<string, object> model)
    {
        var tableNameUpper = table.ToUpper();
        if (!tableMap.ContainsKey(tableNameUpper)) {
            return false;
        }
        var whereFields = model.Keys.AsList();
        string query = string.Empty;
        string queryTemplate = 
            @"DELETE FROM `{0}`
            WHERE
                {1};";
        List<string> queryWhereFieldArrs = new List<string>();
        foreach (var whereField in whereFields)
        {
            var fieldLower = whereField.ToLower();
            if (tableMap[tableNameUpper].ContainsKey(fieldLower)) {
                queryWhereFieldArrs.Add(string.Format("`{0}` = @{1}", fieldLower, whereField));
            }
        }
        query = string.Format(queryTemplate, 
            tableNameUpper,
            string.Join("\n AND ", queryWhereFieldArrs));
        using (var db = AppDb)
        {   
            await db.Connection.OpenAsync();
            var postResult = await db.Connection.ExecuteAsync(query, model);
            return postResult > 0;
        }
    }

    public async Task<Dictionary<string, object>> GetByIdAsync(string table, int id, string userId)
    {
        var model = new Dictionary<string, object>();
        model["id"] = id;
        model["userId"] = userId;
        QueryModelOnSearch queryModelOnSearch = new QueryModelOnSearch() {
            ExcludeFields = new List<string>{"userId"}
        };
        queryModelOnSearch.WhereFieldQuerys = new Dictionary<string, List<string>>();
        queryModelOnSearch.WhereFieldQuerys["id"] = new List<string>() {EnumSearchFunctions.EQUALS};
        queryModelOnSearch.WhereFieldQuerys["userId"] = new List<string>() {EnumSearchFunctions.EQUALS};
        return await GetAsync(table, model, queryModelOnSearch);
    }

    public async Task<Dictionary<string, object>> GetAsync(string table, Dictionary<string, object> model,QueryModelOnSearch queryModelOnSearch)
    {
        if (queryModelOnSearch != null) {
            queryModelOnSearch.MaxRecordCount = 1;
        }
        return (await ListAsync(table, model, queryModelOnSearch)).FirstOrDefault();
    }

    public async Task<IEnumerable<Dictionary<string, object>>> ListAsync(string table, Dictionary<string, object> model, QueryModelOnSearch queryModelOnSearch)
    {
        var tableNameUpper = table.ToUpper();
        if (!tableMap.ContainsKey(tableNameUpper)) {
            return null;
        }
        var queryModel = queryModelOnSearch;
        if (queryModel == null) {
            queryModel = new QueryModelOnSearch();
            queryModel.WhereFieldQuerys = new Dictionary<string, List<string>>();
            var keys = model.Keys.AsList();
            foreach (var key in keys)
            {
                queryModel.WhereFieldQuerys[key.ToLower()] = new List<string>() {EnumSearchFunctions.EQUALS};
            }
        }
        var whereFieldQuerys = LowerKeysOfModel(queryModel.WhereFieldQuerys);
        var onlySelectFields = queryModel.OnlySelectFields;
        var excludeFields = queryModel.ExcludeFields;
        var maxRecordCount = queryModel.MaxRecordCount;
        var whereFields = whereFieldQuerys.Keys.AsList();
        string query = string.Empty;
        string queryTemplate = 
            @"SELECT 
                {0} 
            FROM `{1}`
            WHERE
                {2}
            {4}
            {3};";
        List<string> queryWhereFieldArrs = new List<string>();
        List<string> queryOrderFieldArrs = new List<string>();
        foreach (var whereField in whereFields)
        {
            var fieldLower = whereField.ToLower();
            if (tableMap[tableNameUpper].ContainsKey(fieldLower)) {
                if (whereFieldQuerys.ContainsKey(fieldLower) && whereFieldQuerys[fieldLower].Any()) {
                    var oper = whereFieldQuerys[fieldLower][0];
                    if (oper== EnumSearchFunctions.EQUALS) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` = @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.BETWEENS) {
                        queryWhereFieldArrs.Add(string.Format("`{0}` >= @{1} AND `{0}` <= @{2}", fieldLower, whereFieldQuerys[fieldLower][1].ToLower(), whereFieldQuerys[fieldLower][2].ToLower()));
                    } else if (oper== EnumSearchFunctions.ZERO_THEN_NOT_FILTER_BY_THIS) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("(@{1} = 0 OR `{0}` >= @{1})", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.IN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` IN @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.BIGGER_THAN_TODAY) {
                        queryWhereFieldArrs.Add(string.Format("DATE(`{0}`) >= DATE(NOW())", fieldLower));
                    } else if (oper== EnumSearchFunctions.SMALLER_THAN_TODAY) {
                        queryWhereFieldArrs.Add(string.Format("DATE(`{0}`) <= DATE(NOW())", fieldLower));
                    } else if (oper== EnumSearchFunctions.IS_TODAY) {
                        queryWhereFieldArrs.Add(string.Format("DATE(`{0}`) = DATE(NOW())", fieldLower));
                    } else if (oper== EnumSearchFunctions.SMALLER_THAN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` < @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.SMALLER_OR_EQUALS_THAN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` <= @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.BIGGER_THAN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` > @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.BIGGER_OR_EQUALS_THAN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` >= @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.ORDER) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : "asc";
                        queryOrderFieldArrs.Add(string.Format("`{0}` {1}", fieldLower, paramName));
                    }
                }
            }
        }
        string querySelectFields = string.Empty;
        List<string> querySelectFieldArrs = new List<string>();
        if (onlySelectFields != null && onlySelectFields.Any()) {
            foreach (var field in onlySelectFields)
            {
                var fieldLower = field.ToLower();
                if (tableMap[tableNameUpper].ContainsKey(fieldLower)) {
                    querySelectFieldArrs.Add(string.Format("`{0}`", fieldLower));
                }
            }
            querySelectFields = string.Join(", ", querySelectFieldArrs);
        } else {
            querySelectFields = "*";
        }
        string orderQuery = queryOrderFieldArrs.Any() ? "\n ORDER BY " + string.Join(",\n ", queryOrderFieldArrs) : "";
        query = string.Format(queryTemplate, 
            querySelectFields,
            tableNameUpper,
            queryWhereFieldArrs.Any() ? string.Join("\n AND ", queryWhereFieldArrs) : "1=1",
            maxRecordCount == 0 ? string.Empty : " LIMIT " + maxRecordCount,
            orderQuery
            );
        using (var db = AppDb)
        {   
            await db.Connection.OpenAsync();
            var results = await db.Connection.QueryAsync<dynamic>(query, LowerKeysOfModel(model));
            var objects = new List<Dictionary<string, object>>();
            foreach (var item in results)
            {
                var newItem = new Dictionary<string, object>();
                IDictionary<string,object> o = item;
                var propertyNames = o.Keys;
                foreach (var propName in propertyNames)
                {   
                    if (excludeFields != null && excludeFields.Any(f => f.ToLower() == propName.ToLower())) {
                        continue;
                    }
                    object propValue = o[propName];
                    newItem[propName] = propValue;
                }
                objects.Add(newItem);
            }
            await AddRelatedObjects(table, objects, db);
            return objects;
        }
    }

    public async Task<int> CountAsync(string table, Dictionary<string, object> model, QueryModelOnSearch queryModelOnSearch)
    {
        var tableNameUpper = table.ToUpper();
        if (!tableMap.ContainsKey(tableNameUpper)) {
            return 0;
        }
        var queryModel = queryModelOnSearch;
        if (queryModel == null) {
            queryModel = new QueryModelOnSearch();
            queryModel.WhereFieldQuerys = new Dictionary<string, List<string>>();
            var keys = model.Keys.AsList();
            foreach (var key in keys)
            {
                queryModel.WhereFieldQuerys[key.ToLower()] = new List<string>() {EnumSearchFunctions.EQUALS};
            }
        }
        var whereFieldQuerys = LowerKeysOfModel(queryModel.WhereFieldQuerys);
        var onlySelectFields = queryModel.OnlySelectFields;
        var excludeFields = queryModel.ExcludeFields;
        var maxRecordCount = queryModel.MaxRecordCount;
        var whereFields = whereFieldQuerys.Keys.AsList();
        string query = string.Empty;
        string queryTemplate = 
            @"SELECT 
                {0} 
            FROM `{1}`
            WHERE
                {2}
            {3};";
        List<string> queryWhereFieldArrs = new List<string>();
        foreach (var whereField in whereFields)
        {
            var fieldLower = whereField.ToLower();
            if (tableMap[tableNameUpper].ContainsKey(fieldLower)) {
                if (whereFieldQuerys.ContainsKey(fieldLower) && whereFieldQuerys[fieldLower].Any()) {
                    var oper = whereFieldQuerys[fieldLower][0];
                    if (oper== EnumSearchFunctions.EQUALS) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` = @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.BETWEENS) {
                        queryWhereFieldArrs.Add(string.Format("`{0}` >= @{1} AND `{0}` <= @{2}", fieldLower, whereFieldQuerys[fieldLower][1].ToLower(), whereFieldQuerys[fieldLower][2].ToLower()));
                    } else if (oper== EnumSearchFunctions.ZERO_THEN_NOT_FILTER_BY_THIS) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("(@{1} = 0 OR `{0}` >= @{1})", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.IN) {
                        var paramName = whereFieldQuerys[fieldLower].Count() > 1 
                            ? whereFieldQuerys[fieldLower][1].ToLower()
                            : fieldLower;
                        queryWhereFieldArrs.Add(string.Format("`{0}` IN @{1}", fieldLower, paramName));
                    } else if (oper== EnumSearchFunctions.BIGGER_THAN_TODAY) {
                        queryWhereFieldArrs.Add(string.Format("DATE(`{0}`) >= DATE(NOW())", fieldLower));
                    } else if (oper== EnumSearchFunctions.SMALLER_THAN_TODAY) {
                        queryWhereFieldArrs.Add(string.Format("DATE(`{0}`) <= DATE(NOW())", fieldLower));
                    } else if (oper== EnumSearchFunctions.IS_TODAY) {
                        queryWhereFieldArrs.Add(string.Format("DATE(`{0}`) = DATE(NOW())", fieldLower));
                    }
                }
            }
        }
        string querySelectFields = "count(*)";
        
        query = string.Format(queryTemplate, 
            querySelectFields,
            tableNameUpper,
            queryWhereFieldArrs.Any() ? string.Join("\n AND ", queryWhereFieldArrs) : "1=1",
            maxRecordCount == 0 ? string.Empty : " LIMIT " + maxRecordCount);
        using (var db = AppDb)
        {   
            await db.Connection.OpenAsync();
            var result = await db.Connection.QueryFirstAsync<int>(query, LowerKeysOfModel(model));
            return result;
        }
    }

    private async Task AddRelatedObjects(string table, List<Dictionary<string, object>> objects, AppDb db) {
        await AddRelatedObjects(objects, "contactId", "contact", db);
        await AddRelatedObjects(objects, "productId", "product", db);
        await AddRelatedObjects(objects, "storeId", "store", db);
        await AddRelatedObjects(objects, "productTypeId", "product_type", db);
        await AddRelatedObjects(objects, "receivedNoteId", "received_note", db);
        await AddRelatedObjects(objects, "transferNoteId", "transfer_note", db);
        await AddRelatedObjects(objects, "orderId", "order", db);
        await AddRelatedObjects(objects, "debtId", "debt", db);
        await AddRelatedObjects(objects, "collaboratorId", "staff", db);
        await AddRelatedObjects(objects, "tableId", "table", db);
        await AddRelatedObjects(objects, "moneyAccount", "money_account", db);
        await AddRelatedObjects(objects, "moneyAccountId", "money_account", db, "moneyAccount");
        await AddRelatedObjects(objects, "exportMoneyAccountId", "money_account", db, "exportMoneyAccount");
        await AddRelatedObjects(objects, "importMoneyAccountId", "money_account", db, "importMoneyAccount");
        await AddRelatedObjects(objects, "shipperId", "contact", db, "shipper");
        await AddRelatedObjects(objects, "staffId", "staff", db);
        await AddRelatedObjects(objects, "shiftId", "shift", db);
        await AddRelatedObjects(objects, "categoryId", "trade_category", db, "category");
        await AddRelatedObjects(objects, "businessTypeId", "business_type", db, "businessType");
        await AddRelatedObjects(objects, "salesLineId", "sales_line", db, "salesLine");
        await AddRelatedObjects(objects, "pointConfigId", "point_config", db, "pointConfig");
        await AddRelatedObjects(objects, "convertedOrderId", "order", db, "convertedOrder");
        await AddRelatedObjects(objects, "levelId", "level_config", db, "level");
        await AddRelatedObjects(objects, "promotionId", "promotion", db, "promotion");
        await AddRelatedObjects(objects, "promotionProductId", "product", db, "promotionProduct");
        await AddRelatedObjects(objects, "promotionCategoryId", "trade_category", db, "promotionCategory");
    } 

    private async Task AddRelatedObjects(List<Dictionary<string, object>> objects, string fromField, string toTable,  AppDb db, string toField = "") {
        if (objects == null || !objects.Any()) {
            return;
        }
        var ids = new List<string>();
        foreach (var obj in objects)
        {
            if (obj.ContainsKey(fromField)) {
                ids.Add(obj[fromField].ToString());
            }
        }
        if (!ids.Any()) {
            return;
        }
        var model = new Dictionary<string, object>();
        model["ids"] = ids;
        var searchQuery = new QueryModelOnSearch();
        searchQuery.WhereFieldQuerys = new Dictionary<string, List<string>>();
        searchQuery.WhereFieldQuerys["id"] = new List<string>() {EnumSearchFunctions.IN, "ids"};
        var items = await ListAsync(toTable, model, searchQuery);
        if (items == null || !items.Any()) {
            return;
        }
        foreach (var obj in objects)
        {
            if (!obj.ContainsKey(fromField)) {
                continue;
            }            
            obj[string.IsNullOrEmpty(toField) ? toTable : toField] = items.FirstOrDefault(c => c.ContainsKey("id") && c["id"].ToString() == obj[fromField].ToString());
        }
    }

    private Dictionary<string, object> LowerKeysOfModel(Dictionary<string, object> model) {
        if (model == null) {
            return null;
        }
        var newModel = new Dictionary<string, object>();
        var keys = model.Keys.AsList();
        foreach (var key in keys)
        {
            newModel[key.ToLower()] = model[key];
        }
        return newModel;
    }

    private Dictionary<string, string> LowerKeysOfModel(Dictionary<string, string> model) {
        if (model == null) {
            return null;
        }
        var newModel = new Dictionary<string, string>();
        var keys = model.Keys.AsList();
        foreach (var key in keys)
        {
            newModel[key.ToLower()] = model[key];
        }
        return newModel;
    }

    private Dictionary<string, List<string>> LowerKeysOfModel(Dictionary<string, List<string>> model) {
        if (model == null) {
            return null;
        }
        var newModel = new Dictionary<string, List<string>>();
        var keys = model.Keys.AsList();
        foreach (var key in keys)
        {
            newModel[key.ToLower()] = model[key];
        }
        return newModel;
    }
}