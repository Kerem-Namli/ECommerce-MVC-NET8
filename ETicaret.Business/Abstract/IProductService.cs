using ETicaret.Business.Utilities.Results;
using ETicaret.Entity.DTOs;

namespace ETicaret.Business.Abstract
{
    public interface IProductService
    {
        Task<IDataResult<List<ProductDto>>> GetAllProductsAsync();
        Task<IDataResult<List<ProductDto>>> GetActiveProductsAsync();
        Task<IDataResult<List<ProductDto>>> GetFeaturedProductsAsync();
        Task<IDataResult<List<ProductDto>>> GetProductsByCategoryAsync(int categoryId);
        Task<IDataResult<List<ProductDto>>> SearchProductsAsync(string searchTerm);
        Task<IDataResult<List<ProductDto>>> FilterProductsAsync(int? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy);
        Task<IDataResult<ProductDto>> GetProductByIdAsync(int id);
        Task<IResult> CreateProductAsync(ProductAddDto productDto);
        Task<IResult> UpdateProductAsync(ProductUpdateDto productDto);
        Task<IResult> DeleteProductAsync(int id);
        Task<IResult> UpdateStockAsync(int productId, int quantity);
    }
}
