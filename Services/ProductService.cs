using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ITradeRepository _tradeRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IReceivedNoteRepository _receivedNoteRepository;

    public ProductService(
        IProductRepository productRepository,
        ITradeRepository tradeRepository,
        IOrderRepository orderRepository,
        IReceivedNoteRepository receivedNoteRepository
    )
    {
        _productRepository = productRepository;
        _tradeRepository = tradeRepository;
        _orderRepository = orderRepository;
        _receivedNoteRepository = receivedNoteRepository;
    }

    public async Task<IEnumerable<Product>> GetProducts(string userId, int storeId, bool isMaterial, int categoryId)
    {
        return await _productRepository.GetProducts(userId, storeId, isMaterial, categoryId);
    }

    public async Task<IEnumerable<Product>> GetProductsByIds(string userId, IEnumerable<int> ids) {
        return await _productRepository.GetProductsByIds(userId, ids);
    }
    
    public async Task<IEnumerable<Product>> GetProductsWithExpiry(string userId, int storeId, bool isMaterial, int categoryId)
    {
        return await _productRepository.GetProductsWithExpiry(userId, storeId, isMaterial, categoryId);
    }

    public async Task<IEnumerable<Product>> GetProductsWithQuantity(string userId, int storeId, bool isMaterial, int categoryId)
    {
        return await _productRepository.GetProductsWithQuantity(userId, storeId, isMaterial, categoryId);
    }

    public async Task<Product> GetProduct(int id, string userId, int storeId)
    {
        var post = await _productRepository.GetById(id, userId, storeId);
        return post;
    }

    public async Task<ProductStoreQuantity> GetProductStoreQuantity(int productId, int storeId, string userId)
    {
        var post = await _productRepository.GetProductStoreQuantity(productId, storeId, userId);
        return post;
    }

    public async Task<ProductNote> GetProductNote(int id, string userId)
    {
        var post = await _productRepository.GetProductNoteById(id, userId);
        return post;
    }

    public async Task<bool> RemoveProduct(int id, string userId)
    {
        var post = await _productRepository.Remove(id, userId);
        return post;
    }

    public async Task<int> SaveProduct(Product product, bool isUpdatingDate)
    {
        return await _productRepository.SaveProduct(product, isUpdatingDate);
    }

    public async Task<Product> SearchProductByBarcode(string barcode, string userId)
    {
        var post = await _productRepository.SearchProductByBarcode(barcode, userId);
        return post;
    }

    public async Task<IEnumerable<ProductNote>> GetNotes(string userId, DateTime? dateFrom, DateTime? dateTo, int contactId, int productId, int orderId, int receivedNoteId, int tradeId, int storeId, int transferNoteId, int staffId)
    {
        return await _productRepository.GetNotes(userId,
        dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6),
        dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3),
        contactId, productId, orderId, receivedNoteId, tradeId, storeId, transferNoteId, staffId);
    }

    public async Task<ProductNote> GetProductNoteById(int productNoteId, string userId) {
        var note = await _productRepository.GetProductNoteById(productNoteId, userId);
        return note;
    }

    public async Task<bool> RemoveProductNote(int productNoteId, string userId, bool allowQuantityNegative = false)
    {
        var note = await _productRepository.GetProductNoteById(productNoteId, userId);
        if (note == null)
        {
            return false;
        }
        var product = await GetProduct(note.ProductId, note.UserId, note.StoreId);
        if (product == null)
        {
            return false;
        }

        var sign = 1; // -1 là bán
        if (note.TradeId > 0 && note.Amount > 0 || note.OrderId > 0)
        {
            sign = -1;
        }

        var arr = new List<Task>();

        var quantityMore = sign * note.Quantity;
        if (note.BasicUnit != note.Unit 
            && note.UnitExchange.HasValue
            && note.UnitExchange.Value != 0) {
                quantityMore = quantityMore * note.UnitExchange.Value;
        }
        
        if (note.StoreId == 0)
        {
            product.Count -= quantityMore;
            if (product.Count < 0 && !allowQuantityNegative)
            {
                product.Count = 0;
            }
            arr.Add(_productRepository.SaveProduct(product, false));
        }
        else
        {
            var productQuantity = await GetProductStoreQuantity(note.ProductId, note.StoreId, note.UserId);
            if (productQuantity != null)
            {
                productQuantity.Quantity -= quantityMore;
            }
            else
            {
                productQuantity = new ProductStoreQuantity()
                {
                    Quantity = -quantityMore,
                    ProductId = note.ProductId,
                    StoreId = note.StoreId,
                    UserId = note.UserId
                };
            }
            if (productQuantity.Quantity < 0 && !allowQuantityNegative)
            {
                productQuantity.Quantity = 0;
            }
            arr.Add(_productRepository.SaveProductStoreQuantity(productQuantity, false));
        }

        Task.WaitAll(arr.ToArray());

        return await _productRepository.RemoveProductNote(productNoteId, userId);
    }

    public async Task<int> SaveProductNote(ProductNote note)
    {
        if (note.Id > 0)
        {
            return await _productRepository.SaveProductNote(note);
        }

        var product = await GetProduct(note.ProductId, note.UserId, note.StoreId);
        if (product == null)
        {
            product = await _productRepository.GetByCode(note.ProductCode, note.UserId);
            if (product == null)
            {
                return 0;
            }
        }

        var createdAt = note.Id > 0 ? note.CreatedAt : DateTime.Now;
        if (note.TradeId > 0)
        {
            var item = await _tradeRepository.GetById(note.TradeId, note.UserId);
            createdAt = item.CreatedAt;
        }
        else if (note.OrderId > 0)
        {
            var item = await _orderRepository.GetById(note.OrderId, note.UserId);
            createdAt = item.CreatedAt.HasValue ? item.CreatedAt.Value : DateTime.Now;
        }
        else if (note.ReceivedNoteId > 0)
        {
            var item = await _receivedNoteRepository.GetById(note.ReceivedNoteId, note.UserId);
            createdAt = item.CreatedAt.HasValue ? item.CreatedAt.Value : DateTime.Now;
        }
        note.CreatedAt = createdAt;

        var sign = 1; // -1 là bán
        if (note.TradeId > 0 && note.Amount > 0 || note.OrderId > 0)
        {
            sign = -1;
        }

        var arr = new List<Task>();

        var quantityMore = sign * note.Quantity;
        if (note.BasicUnit != note.Unit
            && note.UnitExchange.HasValue
            && note.UnitExchange.Value != 0) {
                quantityMore = quantityMore * note.UnitExchange.Value;
        }
        if (note.StoreId == 0)
        {
            product.Count += quantityMore;
            arr.Add(_productRepository.SaveProduct(product, false));
        }
        else 
        {
            var productQuantity = await GetProductStoreQuantity(note.ProductId, note.StoreId, note.UserId);
            if (productQuantity != null)
            {
                productQuantity.Quantity += quantityMore;
            }
            else
            {
                productQuantity = new ProductStoreQuantity()
                {
                    Quantity = quantityMore,
                    ProductId = note.ProductId,
                    StoreId = note.StoreId,
                    UserId = note.UserId
                };
            }
            arr.Add(_productRepository.SaveProductStoreQuantity(productQuantity, false));
        }

        Task.WaitAll(arr.ToArray());

        return await _productRepository.SaveProductNote(note);
    }
}