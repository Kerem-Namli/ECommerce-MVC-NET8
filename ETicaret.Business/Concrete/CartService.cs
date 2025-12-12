using ETicaret.Business.Abstract;
using ETicaret.Business.Constants;
using ETicaret.Business.Utilities.Results;
using ETicaret.Data.Abstract;
using ETicaret.Entity.Concrete;
using ETicaret.Entity.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.Business.Concrete
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IDataResult<CartDto>> GetCartByUserIdAsync(string userId)
        {
            var cart = await _unitOfWork.Carts.GetQueryable()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Return empty cart instead of error to allow checkout page to load empty state or handle logic
                return new SuccessDataResult<CartDto>(new CartDto 
                { 
                    CartItems = new List<CartItemDto>(),
                    UserId = userId
                }); 
            }

            var cartDto = new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = cart.CartItems.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Name ?? "",
                    ProductImageUrl = ci.Product?.ImageUrl ?? "",
                    UnitPrice = ci.UnitPrice,
                    Quantity = ci.Quantity
                }).ToList()
            };

            return new SuccessDataResult<CartDto>(cartDto);
        }

        private async Task<Cart> GetOrCreateCartEntityAsync(string userId)
        {
            var cart = await _unitOfWork.Carts.GetQueryable()
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }
            
            return cart;
        }

        public async Task<IDataResult<CartItemDto>> AddToCartAsync(string userId, int productId, int quantity)
        {
            var cart = await GetOrCreateCartEntityAsync(userId);
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            
            if (product == null)
                return new ErrorDataResult<CartItemDto>(Messages.ProductNotFound);

            var existingItem = await _unitOfWork.CartItems.GetQueryable()
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity > product.Stock)
                    return new ErrorDataResult<CartItemDto>($"Stok yetersiz. Mevcut stok: {product.Stock}");

                existingItem.Quantity += quantity;
                existingItem.UpdatedDate = DateTime.Now;
                _unitOfWork.CartItems.Update(existingItem);
                await _unitOfWork.SaveChangesAsync();
                
                return new SuccessDataResult<CartItemDto>(new CartItemDto
                {
                    Id = existingItem.Id,
                    ProductId = existingItem.ProductId,
                    ProductName = product.Name ?? "",
                    ProductImageUrl = product.ImageUrl ?? "",
                    UnitPrice = existingItem.UnitPrice,
                    Quantity = existingItem.Quantity
                }, Messages.ItemAddedToCart);
            }

            if (quantity > product.Stock)
                return new ErrorDataResult<CartItemDto>($"Stok yetersiz. Mevcut stok: {product.Stock}");

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.DiscountPrice ?? product.Price,
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.CartItems.AddAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return new SuccessDataResult<CartItemDto>(new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = product.Name,
                ProductImageUrl = product.ImageUrl,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity
            }, Messages.ItemAddedToCart);
        }

        public async Task<IDataResult<CartItemDto>> UpdateCartItemAsync(int cartItemId, int quantity)
        {
            var cartItem = await _unitOfWork.CartItems.GetQueryable()
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
                
            if (cartItem == null) return new ErrorDataResult<CartItemDto>(Messages.CartItemNotFound);

            if (quantity <= 0)
            {
                _unitOfWork.CartItems.Delete(cartItem);
                await _unitOfWork.SaveChangesAsync();
                return new SuccessDataResult<CartItemDto>(Messages.ItemRemovedFromCart);
            }
            else
            {
                if (quantity > cartItem.Product.Stock)
                    return new ErrorDataResult<CartItemDto>($"Stok yetersiz. Mevcut stok: {cartItem.Product.Stock}");

                cartItem.Quantity = quantity;
                cartItem.UpdatedDate = DateTime.Now;
                _unitOfWork.CartItems.Update(cartItem);
                await _unitOfWork.SaveChangesAsync();
                
                return new SuccessDataResult<CartItemDto>(new CartItemDto
                {
                    Id = cartItem.Id,
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product?.Name ?? "",
                    ProductImageUrl = cartItem.Product?.ImageUrl ?? "",
                    UnitPrice = cartItem.UnitPrice,
                    Quantity = cartItem.Quantity
                }, Messages.CartUpdated);
            }
        }

        public async Task<IResult> RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(cartItemId);
            if (cartItem == null) return new ErrorResult(Messages.CartItemNotFound);

            _unitOfWork.CartItems.Delete(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return new SuccessResult(Messages.ItemRemovedFromCart);
        }

        public async Task<IResult> ClearCartAsync(string userId)
        {
            var cart = await _unitOfWork.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return new ErrorResult(Messages.CartItemNotFound);

            var cartItems = await _unitOfWork.CartItems.FindAsync(ci => ci.CartId == cart.Id);
            _unitOfWork.CartItems.DeleteRange(cartItems);
            await _unitOfWork.SaveChangesAsync();
            return new SuccessResult(Messages.CartCleared);
        }

        public async Task<IDataResult<int>> GetCartItemCountAsync(string userId)
        {
            var cart = await _unitOfWork.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return new SuccessDataResult<int>(0);

            var count = await _unitOfWork.CartItems.CountAsync(ci => ci.CartId == cart.Id);
            return new SuccessDataResult<int>(count);
        }

        public async Task<IDataResult<decimal>> GetCartTotalAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (!cart.Success || cart.Data == null) return new SuccessDataResult<decimal>(0);

            return new SuccessDataResult<decimal>(cart.Data.GrandTotal);
        }
    }
}
