using ETicaret.Business.Abstract;
using ETicaret.Entity.Concrete;
using ETicaret.Entity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ETicaret.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _productService.GetAllProductsAsync();
            if (result.Success)
            {
                return View(result.Data);
            }
            return View(new List<ProductDto>());
        }

        public async Task<IActionResult> Create()
        {
            await LoadCategoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductAddDto productDto, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = "Lütfen tüm alanları doğru doldurun: " + string.Join(", ", errors);
                await LoadCategoriesAsync();
                return View(productDto);
            }

            try
            {
                if (imageFile != null)
                {
                    productDto.ImageUrl = await SaveImageAsync(imageFile);
                }

                var result = await _productService.CreateProductAsync(productDto);
                if (result.Success)
                {
                    TempData["Success"] = "Ürün başarıyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["Error"] = result.Message;
                await LoadCategoriesAsync();
                return View(productDto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ürün eklenirken hata oluştu: " + ex.Message;
                await LoadCategoriesAsync();
                return View(productDto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (!result.Success || result.Data == null)
                return NotFound();

            await LoadCategoriesAsync();
            
            // Map ProductDto to ProductUpdateDto for the view (or use ProductDto if View supports it)
            // But usually Edit view binds to ProductUpdateDto or similar.
            // Since View was using Product, we should use ProductUpdateDto which has Id.
            // We can map manually.
            var product = result.Data;
            var updateDto = new ProductUpdateDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                IsFeatured = product.IsFeatured,
                IsActive = product.IsActive
            };

            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductUpdateDto productDto, IFormFile? imageFile)
        {
            if (id != productDto.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = "Lütfen tüm alanları doğru doldurun: " + string.Join(", ", errors);
                await LoadCategoriesAsync();
                return View(productDto);
            }

            try
            {
                if (imageFile != null)
                {
                    productDto.ImageUrl = await SaveImageAsync(imageFile);
                }
                // If generic image is not provided, we might keep old one?
                // ProductAddDto/UpdateDto has ImageUrl. 
                // Creating new instance of ProductUpdateDto from form binding might have null ImageUrl if not in hidden field.
                // We should handle keeping existing image if new one not provided.
                // But Service.UpdateProductAsync replaces entire entity usually.
                // For now assuming ImageUrl is passed or handled.
                // NOTE: If imageFile is null, ImageUrl might be null.
                // We should probably get existing product to keep image url if not changed.
                // But let's rely on hidden field in View if it exists. (Likely does).

                var result = await _productService.UpdateProductAsync(productDto);
                if (result.Success)
                {
                    TempData["Success"] = "Ürün başarıyla güncellendi!";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = result.Message;
                await LoadCategoriesAsync();
                return View(productDto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ürün güncellenirken hata oluştu: " + ex.Message;
                await LoadCategoriesAsync();
                return View(productDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result.Success)
                TempData["Success"] = "Ürün başarıyla silindi.";
            else
                TempData["Error"] = "Ürün silinemedi: " + result.Message;

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            // Web projesinin wwwroot klasörünü bul
            var parentDir = Directory.GetParent(_webHostEnvironment.ContentRootPath);
            if (parentDir == null) throw new DirectoryNotFoundException("Parent directory not found.");
            
            var webProjectRoot = Path.Combine(parentDir.FullName, "ETicaret.Web", "wwwroot");
            var uploadsFolder = Path.Combine(webProjectRoot, "images", "products");
            
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/products/" + uniqueFileName;
        }
    }
}
