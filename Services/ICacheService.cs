using System.Collections.Generic;

public interface ICacheService
{
    void RemoveListEqualItem(string table, string userId);
    void RemoveGetByIdItem(string table, string userId, string id);
    bool ListEqualContains(string table, string userId);
    bool GetByIdContains(string table, string userId, int id);
    void SetListEqualItem(string table, string userId, Dictionary<string, IEnumerable<Dictionary<string, object>>> value);
    void SetGetByIdItem(string table, string userId, int id, Dictionary<string, object> value);
    Dictionary<string, object> GetGetByIdItem(string table, string userId, int id);
    Dictionary<string, IEnumerable<Dictionary<string, object>>> GetListEqualItem(string table, string userId);
    object GetCacheItem(string table);
    object GetCacheList(string table);
}