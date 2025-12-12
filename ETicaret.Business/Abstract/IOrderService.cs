using ETicaret.Entity.Concrete;

namespace ETicaret.Business.Abstract
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> CreateOrderFromCartAsync(string userId, string shippingAddress, string shippingCity, string shippingDistrict, string? shippingPostalCode, string shippingPhone, string paymentMethod);
        Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<bool> CancelOrderAsync(int orderId);
        Task<bool> MarkAsPaidAsync(int orderId);
    }
}
