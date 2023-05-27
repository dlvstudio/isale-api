using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IOrderRepository {

    Task<IEnumerable<Order>> GetOrders(string userId, DateTime dateFrom, DateTime dateTo, int contactId, int staffId, int storeId, int? status, IEnumerable<int> orderIds = null);

    Task<Order> GetById(int orderId, string userId);
    
    Task<Order> GetByCode(string orderCode, string userId);

    Task<bool> Remove(int orderId, string userId);

    Task<int> SaveOrder(Order order);
    
    Task<int> SaveOrderStatus(Order order);
}