using ETicaret.Business.Abstract;
using ETicaret.Entity.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.Web.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<AppUser> _userManager;

        public CheckoutController(
            ICartService cartService,
            IOrderService orderService,
            UserManager<AppUser> userManager)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var result = await _cartService.GetCartByUserIdAsync(userId!);
            
            if (!result.Success || result.Data == null || !result.Data.CartItems.Any())
            {
                TempData["Error"] = result.Success ? "Sepetiniz boş." : result.Message;
                return RedirectToAction("Index", "Cart");
            }

            var cart = result.Data;
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Cart = cart;
            var totalResult = await _cartService.GetCartTotalAsync(userId!);
            ViewBag.CartTotal = totalResult.Success ? totalResult.Data : 0;
            
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            
            try
            {
                var order = await _orderService.CreateOrderFromCartAsync(
                    userId!,
                    model.Address,
                    model.City,
                    model.District,
                    model.PostalCode,
                    model.Phone,
                    model.PaymentMethod
                );

                TempData["Success"] = "Siparişiniz başarıyla oluşturuldu!";
                return RedirectToAction("Success", new { orderNumber = order.OrderNumber });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Success(string orderNumber)
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null)
                return NotFound();

            return View(order);
        }
    }

    public class CheckoutViewModel
    {
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string District { get; set; } = null!;
        public string? PostalCode { get; set; }
        public string Phone { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
    }
}
