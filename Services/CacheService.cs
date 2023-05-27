using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

public class CacheService : ICacheService
{
    private const string LISTEQUAL = "listequal";
    private const string GETBYID = "getbyid";

    private CacheItem<Product> _productItemCache;
    private CacheItem<ReceivedNote> _receivedNoteItemCache;
    private CacheItem<Order> _orderItemCache;
    private CacheTableItem _tableItemCache;
    private CacheList<Product> _productListCache;
    private CacheList<ReceivedNote> _noteListCache;
    private CacheList<Order> _orderListCache;

    private readonly IMemoryCache _memoryCache;

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _orderItemCache = new CacheItem<Order>(_memoryCache, "ORDER");
        _productItemCache = new CacheItem<Product>(_memoryCache, "PRODUCT");
        _receivedNoteItemCache = new CacheItem<ReceivedNote>(_memoryCache, "RECEIVEDNOTE");
        _tableItemCache = new CacheTableItem(_memoryCache);
        _productListCache = new CacheList<Product>(_memoryCache, "PRODUCT");
        _noteListCache = new CacheList<ReceivedNote>(_memoryCache, "RECEIVEDNOTE");
        _orderListCache = new CacheList<Order>(_memoryCache, "ORDER");
    }

    public void RemoveListEqualItem(string table, string userId)
    {
        _memoryCache.Remove(LISTEQUAL + "-" + table.ToLower() + "-" + userId);
        ClearListEqualCache(userId);
    }


    public void RemoveGetByIdItem(string table, string userId, string id)
    {
        _tableItemCache.RemoveItem(table.ToLower(), userId, id);
    }

    public void SetListEqualItem(string table, string userId, Dictionary<string, IEnumerable<Dictionary<string, object>>> value)
    {
        _memoryCache.Set(LISTEQUAL + "-" + table.ToLower() + "-" + userId, value);
    }

    public Dictionary<string, object> GetGetByIdItem(string table, string userId, int id)
    {
        return _tableItemCache.GetItem(table.ToLower(), userId, id);
    }

    public Dictionary<string, IEnumerable<Dictionary<string, object>>> GetListEqualItem(string table, string userId)
    {
        Dictionary<string, IEnumerable<Dictionary<string, object>>> value = null;
        _memoryCache.TryGetValue<Dictionary<string, IEnumerable<Dictionary<string, object>>>>(LISTEQUAL + "-" + table.ToLower() + "-" + userId, out value);
        return value;
    }

    public void SetGetByIdItem(string table, string userId, int id, Dictionary<string, object> value)
    {
        _tableItemCache.SetItem(table, userId, id, value);
    }

    public object GetCacheItem(string table) {
        if (table == "order")
            return _orderItemCache;
        if (table == "product")
            return _productItemCache;
        if (table == "receivedNote")
            return _receivedNoteItemCache;
        return null;
    }

    public object GetCacheList(string table) {
        if (table == "order")
            return _orderListCache;
        if (table == "product")
            return _productListCache;
        if (table == "receivedNote")
            return _noteListCache;
        return null;
    }

    public bool ListEqualContains(string table, string userId)
    {
        object value = null;
        return _memoryCache.TryGetValue(LISTEQUAL + "-" + table.ToLower() + "-" + userId, out value);
    }

    public bool GetByIdContains(string table, string userId, int id)
    {
        return _tableItemCache.Contains(table.ToLower(), userId, id);
    }

    private void ClearListEqualCache(string userId)
    {
        _orderItemCache.Clear(userId);     
        _productItemCache.Clear(userId);     
        _receivedNoteItemCache.Clear(userId);     
        _memoryCache.Remove(LISTEQUAL + "-contact-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-product-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-store-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-product_type-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-received_note-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-transfer_note-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-order-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-debt-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-staff-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-table-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-money_account-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-trade_category-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-business_type-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-sales_line-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-point_config-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-level_config-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-fbmessage-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-fbmessageflow-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-fbautoorderconfig-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-fbautoreplyconfig-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-fbpost-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-fbpage-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-fbtoken-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-fblivecomment-" + userId);
        _memoryCache.Remove(LISTEQUAL + "-fbcomment-" + userId);
    }
}