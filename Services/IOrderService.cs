using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IOrderService
{
    Task<IEnumerable<Order>> GetOrders(string userId, DateTime? dateFrom, DateTime? dateTo, int contactId, int staffId, int storeId, int? status);

    Task<Order> GetOrder(int id, string userId);
    
    Task<Order> GetOrderByCode(string code, string userId);

    Task<bool> RemoveOrder(int id, string userId);

    Task<int> SaveOrder(Order order);
    Task<int> SaveOrderStatus(Order order);
}