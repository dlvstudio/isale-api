using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

public class CacheTableItem
{
    private const string _prefix = "TABLEITEM";

    private readonly IMemoryCache _memoryCache;

    public CacheTableItem(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void RemoveItem(string table, string userId, string id)
    {
        var key = _prefix + "-item-" + table + "-" + userId + "-" + id;
        _memoryCache.Remove(key);

        HashSet<string> keysHashset = null;
        var hasHashSet = _memoryCache.TryGetValue<HashSet<string>>(_prefix + "-" + table + "-" + userId, out keysHashset);
        if (!hasHashSet || keysHashset == null)
        {
            return;
        }
        if (!keysHashset.Contains(key))
        {
            return;
        }
        keysHashset.Remove(key);
        _memoryCache.Set<HashSet<string>>(_prefix + "-" + table + "-" + userId, keysHashset);
    }

    public Dictionary<string, object> GetItem(string table, string userId, int id)
    {
        if (!Contains(table, userId, id)) {
            return null;
        }
        Dictionary<string, object> value;
        _memoryCache.TryGetValue<Dictionary<string, object>>(_prefix + "-item-" + table + "-" + userId + "-" + id, out value);
        return value;
    }

    public void SetItem(string table, string userId, int id, Dictionary<string, object> value)
    {
        var key = _prefix + "-item-" + table + "-" + userId + "-" + id;
        HashSet<string> keysHashset = null;
        var hasHashSet = _memoryCache.TryGetValue<HashSet<string>>(_prefix + "-" + table + "-" + userId, out keysHashset);
        if (!hasHashSet || keysHashset == null)
        {
            keysHashset = new HashSet<string>();
        }
        if (!keysHashset.Contains(key))
        {
            keysHashset.Add(key);
        }
        _memoryCache.Set<HashSet<string>>(_prefix + "-" + table + "-" + userId, keysHashset);
        _memoryCache.Set<Dictionary<string, object>>(key, value);
    }

    public bool Contains(string table, string userId, int id)
    {
        HashSet<string> keysHashset = null;
        var hasHashSet = _memoryCache.TryGetValue<HashSet<string>>(_prefix + "-" + table + "-" + userId, out keysHashset);
        if (!hasHashSet || keysHashset == null)
        {
            return false;
        }
        var key = _prefix + "-item-" + table + "-" + userId + "-" + id;
        return keysHashset.Contains(key);
    }

    public void Clear(string table, string userId) {
        HashSet<string> orderHashset = null;
        _memoryCache.TryGetValue<HashSet<string>>(_prefix + "-" + table + "-" + userId, out orderHashset);
        if (orderHashset != null) {
            foreach (var key in orderHashset)
            {
                _memoryCache.Remove(key); 
            }
        }
        _memoryCache.Remove(_prefix + "-" + table + "-" + userId);   
    }
}