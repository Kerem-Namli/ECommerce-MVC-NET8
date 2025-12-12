using ETicaret.Business.Abstract;
using ETicaret.Entity.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { message = "Sipariş bulunamadı." });

            return Ok(order);
        }

        // GET: api/Orders/number/ORD-20231201-ABC12345
        [HttpGet("number/{orderNumber}")]
        public async Task<ActionResult<Order>> GetOrderByNumber(string orderNumber)
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null)
                return NotFound(new { message = "Sipariş bulunamadı." });

            return Ok(order);
        }

        // GET: api/Orders/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByUser(string userId)
        {
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        // GET: api/Orders/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByStatus(OrderStatus status)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(orders);
        }

        // POST: api/Orders/fromcart
        [HttpPost("fromcart")]
        public async Task<ActionResult<Order>> CreateOrderFromCart([FromBody] CreateOrderRequest request)
        {
            try
            {
                var order = await _orderService.CreateOrderFromCartAsync(
                    request.UserId,
                    request.ShippingAddress,
                    request.ShippingCity,
                    request.ShippingDistrict,
                    request.ShippingPostalCode,
                    request.ShippingPhone,
                    request.PaymentMethod
                );

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new { message = "Sipariş oluşturuldu.", order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Orders/5/status
        [HttpPut("{id}/status")]
        public async Task<ActionResult<Order>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(id, request.Status);
                return Ok(new { message = "Sipariş durumu güncellendi.", order });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Orders/5/pay
        [HttpPut("{id}/pay")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var result = await _orderService.MarkAsPaidAsync(id);
            if (!result)
                return NotFound(new { message = "Sipariş bulunamadı." });

            return Ok(new { message = "Sipariş ödendi olarak işaretlendi." });
        }

        // PUT: api/Orders/5/cancel
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var result = await _orderService.CancelOrderAsync(id);
                if (!result)
                    return NotFound(new { message = "Sipariş bulunamadı." });

                return Ok(new { message = "Sipariş iptal edildi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CreateOrderRequest
    {
        public string UserId { get; set; } = null!;
        public string ShippingAddress { get; set; } = null!;
        public string ShippingCity { get; set; } = null!;
        public string ShippingDistrict { get; set; } = null!;
        public string? ShippingPostalCode { get; set; }
        public string ShippingPhone { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
    }

    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}
