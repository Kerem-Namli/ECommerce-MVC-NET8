using ETicaret.Business.Abstract;
using ETicaret.Business.Constants;
using ETicaret.Business.Utilities.Results;
using ETicaret.Data.Abstract;
using ETicaret.Entity.Concrete;
using ETicaret.Entity.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ETicaret.Business.Concrete
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IDataResult<List<ProductDto>>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Category)
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name ?? "",
                Description = p.Description ?? "",
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl ?? "",
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "",
                IsFeatured = p.IsFeatured,
                IsActive = p.IsActive
            }).ToList();

            return new SuccessDataResult<List<ProductDto>>(productDtos);
        }

        public async Task<IDataResult<List<ProductDto>>> GetActiveProductsAsync()
        {
            var products = await _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name ?? "",
                Description = p.Description ?? "",
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl ?? "",
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "",
                IsFeatured = p.IsFeatured,
                IsActive = p.IsActive
            }).ToList();

            return new SuccessDataResult<List<ProductDto>>(productDtos);
        }

        public async Task<IDataResult<List<ProductDto>>> GetFeaturedProductsAsync()
        {
            var products = await _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsFeatured)
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name ?? "",
                Description = p.Description ?? "",
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl ?? "",
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "",
                IsFeatured = p.IsFeatured,
                IsActive = p.IsActive
            }).ToList();

            return new SuccessDataResult<List<ProductDto>>(productDtos);
        }

        public async Task<IDataResult<List<ProductDto>>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name ?? "",
                Description = p.Description ?? "",
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl ?? "",
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "",
                IsFeatured = p.IsFeatured,
                IsActive = p.IsActive
            }).ToList();

            return new SuccessDataResult<List<ProductDto>>(productDtos);
        }

        public async Task<IDataResult<ProductDto>> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return new ErrorDataResult<ProductDto>(Messages.ProductNotFound);

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name ?? "",
                Description = product.Description ?? "",
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl ?? "",
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "",
                IsFeatured = product.IsFeatured,
                IsActive = product.IsActive
            };

            return new SuccessDataResult<ProductDto>(productDto);
        }

        public async Task<IResult> CreateProductAsync(ProductAddDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                DiscountPrice = productDto.DiscountPrice,
                Stock = productDto.Stock,
                ImageUrl = productDto.ImageUrl,
                CategoryId = productDto.CategoryId,
                IsFeatured = productDto.IsFeatured,
                IsActive = productDto.IsActive,
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return new SuccessResult(Messages.ProductAdded);
        }

        public async Task<IResult> UpdateProductAsync(ProductUpdateDto productDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productDto.Id);
            if (product == null) return new ErrorResult(Messages.ProductNotFound);

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.DiscountPrice = productDto.DiscountPrice;
            product.Stock = productDto.Stock;
            product.ImageUrl = productDto.ImageUrl;
            product.CategoryId = productDto.CategoryId;
            product.IsFeatured = productDto.IsFeatured;
            product.IsActive = productDto.IsActive;
            product.UpdatedDate = DateTime.Now;

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();
            return new SuccessResult(Messages.ProductUpdated);
        }

        public async Task<IResult> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return new ErrorResult(Messages.ProductNotFound);

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.SaveChangesAsync();
            return new SuccessResult(Messages.ProductDeleted);
        }

        public async Task<IResult> UpdateStockAsync(int productId, int quantity)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null) return new ErrorResult(Messages.ProductNotFound);

            product.Stock = quantity;
            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();
            return new SuccessResult(Messages.ProductUpdated);
        }

        public async Task<IDataResult<List<ProductDto>>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetActiveProductsAsync();
            }

            var products = await _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.IsActive && (p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)))
                .ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name ?? "",
                Description = p.Description ?? "",
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl ?? "",
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "",
                IsFeatured = p.IsFeatured,
                IsActive = p.IsActive
            }).ToList();

            return new SuccessDataResult<List<ProductDto>>(productDtos);
        }

        public async Task<IDataResult<List<ProductDto>>> FilterProductsAsync(int? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            var query = _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Sorting
            switch (sortBy?.ToLower())
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    query = query.OrderByDescending(p => p.CreatedDate);
                    break;
                default: // Default sort by Id or Name
                    query = query.OrderBy(p => p.Id);
                    break;
            }

            var products = await query.ToListAsync();

            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name ?? "",
                Description = p.Description ?? "",
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl ?? "",
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "",
                IsFeatured = p.IsFeatured,
                IsActive = p.IsActive
            }).ToList();

            return new SuccessDataResult<List<ProductDto>>(productDtos);
        }
    }
}
