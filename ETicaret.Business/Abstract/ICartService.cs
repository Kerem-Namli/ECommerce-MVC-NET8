using ETicaret.Entity.Concrete;
using ETicaret.Entity.DTOs;
using ETicaret.Business.Utilities.Results;

namespace ETicaret.Business.Abstract
{
    public interface ICartService
    {
        Task<IDataResult<CartDto>> GetCartByUserIdAsync(string userId);
        Task<IDataResult<CartItemDto>> AddToCartAsync(string userId, int productId, int quantity);
        Task<IDataResult<CartItemDto>> UpdateCartItemAsync(int cartItemId, int quantity);
        Task<IResult> RemoveFromCartAsync(int cartItemId);
        Task<IResult> ClearCartAsync(string userId);
        Task<IDataResult<int>> GetCartItemCountAsync(string userId);
        Task<IDataResult<decimal>> GetCartTotalAsync(string userId);
    }
}
