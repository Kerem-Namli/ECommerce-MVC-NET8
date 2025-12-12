using ETicaret.Business.Abstract;
using ETicaret.Entity.Concrete;
using ETicaret.Entity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ETicaret.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly UserManager<AppUser> _userManager;
        private const string CART_COOKIE = "GuestCart";

        public CartController(ICartService cartService, IProductService productService, UserManager<AppUser> userManager)
        {
            _cartService = cartService;
            _productService = productService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var cartResult = await _cartService.GetCartByUserIdAsync(userId);
                var totalResult = await _cartService.GetCartTotalAsync(userId);
                
                if (!cartResult.Success)
                {
                    ViewBag.ErrorMessage = cartResult.Message;
                    return View(new CartDto());
                }

                ViewBag.CartTotal = totalResult.Data;
                return View(cartResult.Data);
            }
            
            // Guest Flow
            var guestCart = GetGuestCart();
            
            // Hydrate product details
            foreach (var item in guestCart.CartItems)
            {
                var productResult = await _productService.GetProductByIdAsync(item.ProductId);
                if (productResult.Success && productResult.Data != null)
                {
                    item.ProductName = productResult.Data.Name;
                    // item.Price does not exist in CartItemDto. We use UnitPrice.
                    item.ProductImageUrl = productResult.Data.ImageUrl;
                    item.Id = item.ProductId; // Use ProductId as fake Id
                    
                    if (productResult.Data.DiscountPrice.HasValue) 
                    {
                        item.UnitPrice = productResult.Data.DiscountPrice.Value;
                    }
                    else
                    {
                        item.UnitPrice = productResult.Data.Price;
                    }
                    // TotalPrice is auto-calculated by property
                }
            }
            
            // GrandTotal is auto-calculated logic (Sum TotalPrice)
            ViewBag.CartTotal = guestCart.GrandTotal;
            return View(guestCart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var result = await _cartService.AddToCartAsync(userId, productId, quantity);
                if (result.Success)
                {
                    var countResult = await _cartService.GetCartItemCountAsync(userId);
                    return Json(new { success = true, message = result.Message, cartCount = countResult.Data });
                }
                return Json(new { success = false, message = result.Message });
            }

            // Guest Flow
            var guestCart = GetGuestCart();
            var existingItem = guestCart.CartItems.FirstOrDefault(x => x.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                guestCart.CartItems.Add(new CartItemDto 
                { 
                    ProductId = productId, 
                    Quantity = quantity
                });
            }
            
            SaveGuestCart(guestCart);
            return Json(new { success = true, message = "Ürün sepete eklendi", cartCount = guestCart.CartItems.Sum(x => x.Quantity) });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var result = await _cartService.UpdateCartItemAsync(cartItemId, quantity);
                if (result.Success)
                {
                    var totalResult = await _cartService.GetCartTotalAsync(userId);
                    var countResult = await _cartService.GetCartItemCountAsync(userId);
                    return Json(new { success = true, total = totalResult.Data.ToString("C2"), cartCount = countResult.Data });
                }
                return Json(new { success = false, message = result.Message });
            }

            // Guest Flow
            var guestCart = GetGuestCart();
            var item = guestCart.CartItems.FirstOrDefault(x => x.ProductId == cartItemId); 
            if (item != null)
            {
                item.Quantity = quantity;
                SaveGuestCart(guestCart);
                
                // Recalculate totals for response
                // We need to fetch prices to calculate accurate total
                decimal total = await CalculateGuestCartTotal(guestCart);

                return Json(new { success = true, total = total.ToString("C2"), cartCount = guestCart.CartItems.Sum(x => x.Quantity) });
            }
            return Json(new { success = false, message = "Ürün bulunamadı" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var result = await _cartService.RemoveFromCartAsync(cartItemId);
                if (result.Success)
                {
                    var totalResult = await _cartService.GetCartTotalAsync(userId);
                    var countResult = await _cartService.GetCartItemCountAsync(userId);
                    return Json(new { success = true, total = totalResult.Data.ToString("C2"), cartCount = countResult.Data });
                }
                return Json(new { success = false, message = result.Message });
            }

            // Guest Flow
            var guestCart = GetGuestCart();
            var item = guestCart.CartItems.FirstOrDefault(x => x.ProductId == cartItemId);
            if (item != null)
            {
                guestCart.CartItems.Remove(item);
                SaveGuestCart(guestCart);
                
                decimal total = await CalculateGuestCartTotal(guestCart);
                
                return Json(new { success = true, total = total.ToString("C2"), cartCount = guestCart.CartItems.Sum(x => x.Quantity) });
            }
             return Json(new { success = false, message = "Ürün bulunamadı" });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var result = await _cartService.GetCartByUserIdAsync(userId);
                if (result.Success && result.Data != null)
                {
                    return Json(new { count = result.Data.CartItems.Sum(x => x.Quantity) });
                }
                return Json(new { count = 0 });
            }
            
            // Guest Flow
            var guestCart = GetGuestCart();
            return Json(new { count = guestCart.CartItems.Sum(x => x.Quantity) });
        }
        
        // --- Helpers ---
        private CartDto GetGuestCart()
        {
            var cookie = Request.Cookies[CART_COOKIE];
            if (string.IsNullOrEmpty(cookie)) return new CartDto { CartItems = new List<CartItemDto>() };
            try {
                return JsonSerializer.Deserialize<CartDto>(cookie) ?? new CartDto { CartItems = new List<CartItemDto>() };
            } catch { return new CartDto { CartItems = new List<CartItemDto>() }; }
        }

        private void SaveGuestCart(CartDto cart)
        {
            var options = new CookieOptions { Expires = DateTime.Now.AddDays(7), HttpOnly = true };
            var json = JsonSerializer.Serialize(cart);
            Response.Cookies.Append(CART_COOKIE, json, options);
        }

        private async Task<decimal> CalculateGuestCartTotal(CartDto guestCart)
        {
            decimal total = 0;
            foreach(var cItem in guestCart.CartItems) {
                 var p = await _productService.GetProductByIdAsync(cItem.ProductId);
                 if(p.Success && p.Data != null) {
                     decimal price = p.Data.DiscountPrice ?? p.Data.Price;
                     total += price * cItem.Quantity;
                 }
            }
            return total;
        }
    }
}
