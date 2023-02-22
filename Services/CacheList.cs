using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

public class CacheList<T>
{
    private const string LIST = "list";
    private const string LISTKEYS = "listkeys";
    private string _prefix = string.Empty;

    private readonly IMemoryCache _memoryCache;

    public CacheList(IMemoryCache memoryCache, string prefix)
    {
        _memoryCache = memoryCache;
        _prefix = prefix;
    }

    public void SetList(IEnumerable<T> items, string userId, string cacheKey) {
        var key = LIST + "-" + _prefix + "-" + userId + "-" + cacheKey;
        _memoryCache.Set<IEnumerable<T>>(LIST + "-" + _prefix + "-" + userId + "-" + cacheKey, items);
        HashSet<string> itemListKeys = null;
        var hasListKeys = _memoryCache.TryGetValue<HashSet<string>>(LISTKEYS + "-" + _prefix + "-" + userId, out itemListKeys);
        if (!hasListKeys || itemListKeys == null) {
            itemListKeys = new HashSet<string>();            
        }
        if (!itemListKeys.Contains(key)) {
            itemListKeys.Add(key);
            _memoryCache.Set<HashSet<string>>(LISTKEYS + "-" + _prefix + "-" + userId, itemListKeys);
        }
    }

    public IEnumerable<T> GetList(string userId, string cacheKey) {
        var key = LIST + "-" + _prefix + "-" + userId + "-" + cacheKey;
        HashSet<string> itemListKeys = null;
        var hasListKeys = _memoryCache.TryGetValue<HashSet<string>>(LISTKEYS + "-" + _prefix + "-" + userId, out itemListKeys);
        if (!hasListKeys || itemListKeys == null) {
            return null;            
        }
        if (!itemListKeys.Contains(key)) {
            return null;       
        }

        IEnumerable<T> products = null;
        var hasList = _memoryCache.TryGetValue<IEnumerable<T>>(key, out products);
        if (hasList) {
            return products;
        }
        return null;
    }

    public void ClearAllLists(string userId) {
        HashSet<string> itemListKeys = null;
        var hasListKeys = _memoryCache.TryGetValue<HashSet<string>>(LISTKEYS + "-" + _prefix + "-" + userId, out itemListKeys);
        if (!hasListKeys || itemListKeys == null) {
            itemListKeys = new HashSet<string>();            
        }
        foreach (var key in itemListKeys)
        {
            _memoryCache.Remove(key);
        }
        _memoryCache.Remove(LISTKEYS + "-" + _prefix + "-" + userId);
    }
}