using ETicaret.Business.Abstract;
using ETicaret.Data.Abstract;
using ETicaret.Entity.Concrete;
using ETicaret.Entity.DTOs;
using ETicaret.Business.Utilities.Results;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.Business.Concrete
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartService _cartService;

        public OrderService(IUnitOfWork unitOfWork, ICartService cartService)
        {
            _unitOfWork = unitOfWork;
            _cartService = cartService;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _unitOfWork.Orders.GetQueryable()
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _unitOfWork.Orders.GetQueryable()
                .Where(o => o.Status == status)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            return await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            order.OrderNumber = GenerateOrderNumber();
            order.CreatedDate = DateTime.Now;
            order.OrderDate = DateTime.Now;
            
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            return order;
        }

        public async Task<Order> CreateOrderFromCartAsync(
            string userId, 
            string shippingAddress, 
            string shippingCity, 
            string shippingDistrict, 
            string? shippingPostalCode, 
            string shippingPhone, 
            string paymentMethod)
        {
            var cartResult = await _cartService.GetCartByUserIdAsync(userId);
            
            if (!cartResult.Success || cartResult.Data == null || !cartResult.Data.CartItems.Any())
                throw new Exception("Sepetiniz boş.");

            var cart = cartResult.Data;

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                ShippingAddress = shippingAddress,
                ShippingCity = shippingCity,
                ShippingDistrict = shippingDistrict,
                ShippingPostalCode = shippingPostalCode,
                ShippingPhone = shippingPhone,
                PaymentMethod = paymentMethod,
                Status = OrderStatus.Pending,
                IsPaid = false,
                CreatedDate = DateTime.Now,
                OrderDate = DateTime.Now
            };

            decimal totalPrice = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in cart.CartItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                if (product == null) continue;

                if (product.Stock < cartItem.Quantity)
                    throw new Exception($"{product.Name} için yeterli stok yok.");

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalPrice = cartItem.UnitPrice * cartItem.Quantity,
                    CreatedDate = DateTime.Now
                };

                totalPrice += orderItem.TotalPrice;
                orderItems.Add(orderItem);

                // Reduce stock
                product.Stock -= cartItem.Quantity;
                
                // Auto-Passive if out of stock
                if (product.Stock <= 0)
                {
                    product.IsActive = false;
                }

                _unitOfWork.Products.Update(product);
            }

            order.TotalPrice = totalPrice;
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            // Add order items
            foreach (var item in orderItems)
            {
                item.OrderId = order.Id;
                await _unitOfWork.OrderItems.AddAsync(item);
            }
            await _unitOfWork.SaveChangesAsync();

            // Clear cart
            await _cartService.ClearCartAsync(userId);

            return order;
        }

        public async Task<Order> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Sipariş bulunamadı.");

            order.Status = status;
            order.UpdatedDate = DateTime.Now;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return order;
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null) return false;

            if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
                throw new Exception("Kargoya verilmiş veya teslim edilmiş siparişler iptal edilemez.");

            // Restore stock
            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    _unitOfWork.Products.Update(product);
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedDate = DateTime.Now;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsPaidAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null) return false;

            order.IsPaid = true;
            order.PaidDate = DateTime.Now;
            order.UpdatedDate = DateTime.Now;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }
    }
}
