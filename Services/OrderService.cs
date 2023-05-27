using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IProductRepository _productRepository;

    public OrderService(
        IOrderRepository repository,
        IProductRepository productRepository
    ) {
        _repository = repository;
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<Order>> GetOrders(string userId, DateTime? dateFrom, DateTime? dateTo, int contactId, int staffId, int storeId, int? status)
    {
        return await _repository.GetOrders(userId, 
            dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6), 
            dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3),
            contactId, staffId, storeId, status);
    }

    public async Task<Order> GetOrderByCode(string code, string userId) {
        var post = await _repository.GetByCode(code, userId);
        return post;
    }

    public async Task<Order> GetOrder(int id, string userId) {
        var post = await _repository.GetById(id, userId);
        return post;
    }

    public async Task<bool> RemoveOrder(int id, string userId) {
        var post = await _repository.Remove(id, userId);
        return post;
    }

    public async Task<int> SaveOrder(Order order) {
        return await _repository.SaveOrder(order);
    }

    public async Task<int> SaveOrderStatus(Order order) {
        return await _repository.SaveOrderStatus(order);
    }
}