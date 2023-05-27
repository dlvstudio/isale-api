using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

public class CacheItem<T>
{
    private string _prefix = string.Empty;

    private readonly IMemoryCache _memoryCache;

    public CacheItem(IMemoryCache memoryCache, string prefix)
    {
        _memoryCache = memoryCache;
        _prefix = prefix;
    }

    public void RemoveItem(string userId, string id)
    {
        var key = _prefix + "-item-" + userId + "-" + id;
        _memoryCache.Remove(key);

        HashSet<string> keysHashset = null;
        var hasHashSet = _memoryCache.TryGetValue<HashSet<string>>(_prefix + "-" + userId, out keysHashset);
        if (!hasHashSet || keysHashset == null)
        {
            return;
        }
        if (!keysHashset.Contains(key))
        {
            return;
        }
        keysHashset.Remove(key);
        _memoryCache.Set<HashSet<string>>(_prefix + "-" + userId, keysHashset);
    }

    public void RemoveItems(string userId, List<string> ids)
    {
        HashSet<string> keysHashset = null;
        var hasHashSet = _memoryCache.TryGetValue<HashSet<string>>(_prefix + "-" + userId, out keysHashset);
        foreach (var id in ids)
        {
            var key = _prefix + "-item-" + userId + "-" + id;
            _memoryCache.Remove(key);   
            if (!hasHashSet || keysHashset == null)
            {
                continue;
            }
            if (keysHashset.Contains(key))
            {
                keysHashset.Remove(key);
            } 
        }
        _memoryCache.Set<HashSet<string>>(_prefix + "-" + userId, keysHashset);
    }

    public T GetItem(string userId, int id)
    {
        if (!Contains(userId, id)) {
            return default(T);
        }
        T value;
        _memoryCache.TryGetValue<T>(_prefix + "-item-" + userId + "-" + id, out value);
        return value;
    }

    public void SetItem(string userId, int id, T value)
    {
        var key = _prefix + "-item-" + userId + "-" + id;
        HashSet<string> keysHashset = null;
        var hasHashSet = _memoryCache.TryGetValue<HashSet<string>>(_prefix + "-" + userId, out keysHashset);
        if (!hasHashSet || keysHashset == null)
        {
            keysHashset = new HashSet<string>();
        }
        if (!keysHashset.Contains(key))
        {
            keysHashset.Add(key);
        }
        _memoryCache.Set<HashSet<string>>(_prefix + "-" + userId, keysHashset);
        _memoryCache.Set<T>(key, value);
    }

    public bool Contains(string userId, int id)
    {
        HashSet<string> keysHashset = null;
        var hasHashSet = _memoryCache.TryGetValue<HashSet<string>>(_prefix + "-" + userId, out keysHashset);
        if (!hasHashSet || keysHashset == null)
        {
            return false;
        }
        var key = _prefix + "-item-" + userId + "-" + id;
        if (!keysHashset.Contains(key)) {
            return false;
        }
        T item;
        return _memoryCache.TryGetValue<T>(key, out item);
    }

    public void Clear(string userId) {
        HashSet<string> orderHashset = null;
        _memoryCache.TryGetValue<HashSet<string>>(_prefix + "-" + userId, out orderHashset);
        if (orderHashset != null) {
            foreach (var key in orderHashset)
            {
                _memoryCache.Remove(key); 
            }
        }
        _memoryCache.Remove(_prefix + "-" + userId);   
    }
}