using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IProductService
{
    Task<IEnumerable<Product>> GetProducts(string userId, int storeId, bool isMaterial, int categoryId);

    Task<IEnumerable<Product>> GetProductsByIds(string userId, IEnumerable<int> ids);

    Task<IEnumerable<Product>> GetProductsWithExpiry(string userId, int storeId, bool isMaterial, int categoryId);

    Task<IEnumerable<Product>> GetProductsWithQuantity(string userId, int storeId, bool isMaterial, int categoryId);

    Task<Product> GetProduct(int id, string userId, int storeId);
    
    Task<ProductStoreQuantity> GetProductStoreQuantity(int productId, int storeId, string userId);

    Task<Product> SearchProductByBarcode(string barcode, string userId);

    Task<bool> RemoveProduct(int id, string userId);

    Task<int> SaveProduct(Product product, bool isUpdatingDate);

    Task<IEnumerable<ProductNote>> GetNotes(string userId, DateTime? dateFrom, DateTime? dateTo, int contactId, int productId, int orderId, int receivedNoteId, int tradeId, int storeId, int transferNoteId, int staffId);

    Task<bool> RemoveProductNote(int productNoteId, string userId, bool allowQuantityNegative = false);

    Task<int> SaveProductNote(ProductNote note);
    
    Task<ProductNote> GetProductNoteById(int productNoteId, string userId);
}