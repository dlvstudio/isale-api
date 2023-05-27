using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IProductRepository {

    Task<IEnumerable<Product>> GetProducts(string userId, int storeId, bool isMaterial, int categoryId);

    Task<IEnumerable<Product>> GetProductsByIds(string userId, IEnumerable<int> ids);

    Task<IEnumerable<Product>> GetProductsWithExpiry(string userId, int storeId, bool isMaterial, int categoryId);

    Task<IEnumerable<Product>> GetProductsWithQuantity(string userId, int storeId, bool isMaterial, int categoryId);

    Task<Product> GetById(int productId, string userId, int storeId);

    Task<ProductStoreQuantity> GetProductStoreQuantity(int productId, int storeId, string userId);

    Task<Product> GetByCode(string productCode, string userId);

    Task<ProductNote> GetProductNoteById(int productNoteId, string userId);

    Task<bool> Remove(int productId, string userId);

    Task<int> SaveProduct(Product product, bool isUpdatingDate);

    Task<int> SaveProductStoreQuantity(ProductStoreQuantity product, bool isUpdatingDate);

    Task<Product> SearchProductByBarcode(string barcode, string userId);

    Task<IEnumerable<ProductNote>> GetNotes(string userId, DateTime dateFrom, DateTime dateTo, int contactId, int productId, int orderId, int receivedNoteId, int tradeId, int storeId, int transferNoteId, int staffId, bool withStaff = false);

    Task<bool> RemoveProductNote(int productNoteId, string userId);

    Task<int> SaveProductNote(ProductNote note);
}