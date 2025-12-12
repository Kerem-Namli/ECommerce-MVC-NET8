using ETicaret.Business.Abstract;
using ETicaret.Entity.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            var result = await _productService.FilterProductsAsync(categoryId, minPrice, maxPrice, sortBy);
            var categories = await _categoryService.GetActiveCategoriesAsync();
            
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;
            
            if (result.Success)
            {
                return View(result.Data);
            }
            // Handle error, maybe return empty list
            return View(new List<ProductDto>());
        }

        public async Task<IActionResult> Details(int id)
        {
            var productResult = await _productService.GetProductByIdAsync(id);
            if (!productResult.Success || productResult.Data == null)
                return NotFound();

            var product = productResult.Data;
            var relatedResult = await _productService.GetProductsByCategoryAsync(product.CategoryId);
            
            if (relatedResult.Success)
            {
                ViewBag.RelatedProducts = relatedResult.Data.Where(p => p.Id != id).Take(4).ToList();
            }
            else
            {
                ViewBag.RelatedProducts = new List<ProductDto>();
            }
            
            return View(product);
        }

        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction(nameof(Index));

            var result = await _productService.SearchProductsAsync(q);
            ViewBag.SearchTerm = q;
            
            if (result.Success)
            {
                return View("Index", result.Data);
            }
            
            return View("Index", new List<ProductDto>());
        }
    }
}
