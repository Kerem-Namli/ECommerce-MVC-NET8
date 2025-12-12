using ETicaret.Business.Abstract;
using ETicaret.Entity.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.Web.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<AppUser> _userManager;

        public OrdersController(IOrderService orderService, UserManager<AppUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var orders = await _orderService.GetOrdersByUserIdAsync(userId!);
            
            // Order by date descending
            var sortedOrders = orders.OrderByDescending(o => o.OrderDate).ToList();
            
            return View(sortedOrders);
        }
    }
}
