using ETicaret.Business.Abstract;
using ETicaret.Entity.DTOs;
using ETicaret.Entity.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(
            IProductService productService, 
            IOrderService orderService,
            ICategoryService categoryService,
            UserManager<AppUser> userManager)
        {
            _productService = productService;
            _orderService = orderService;
            _categoryService = categoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var productsResult = await _productService.GetAllProductsAsync();
            var orders = await _orderService.GetAllOrdersAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();
            var users = _userManager.Users.ToList();

            ViewBag.ProductCount = productsResult.Success ? productsResult.Data.Count : 0;
            ViewBag.OrderCount = orders.Count();
            ViewBag.UserCount = users.Count;
            ViewBag.CategoryCount = categories.Count();
            ViewBag.TotalRevenue = orders.Where(o => o.IsPaid).Sum(o => o.TotalPrice);
            ViewBag.PendingOrders = orders.Count(o => o.Status == ETicaret.Entity.Concrete.OrderStatus.Pending || o.Status == ETicaret.Entity.Concrete.OrderStatus.Processing);
            
            var recentOrders = orders.OrderByDescending(o => o.OrderDate).Take(5).ToList();
            ViewBag.RecentOrders = recentOrders;

            return View();
        }
    }
}
