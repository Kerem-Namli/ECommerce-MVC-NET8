using ETicaret.Business.Abstract;
using ETicaret.Entity.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ETicaret.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public HomeController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var featuredResult = await _productService.GetFeaturedProductsAsync();
            var categories = await _categoryService.GetActiveCategoriesAsync();
            
            ViewBag.Categories = categories;
            
            if (featuredResult.Success)
            {
                return View(featuredResult.Data);
            }
            return View(new List<ProductDto>());
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
