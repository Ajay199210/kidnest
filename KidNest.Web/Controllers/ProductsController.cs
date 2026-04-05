using KidNest.Services.DTOs.Products;
using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.Home;
using KidNest.Web.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductsService _productsService;

        public ProductsController(IProductsService productsService)
        {
            _productsService = productsService;
        }

        // GET: ProductsController/NewReleases
        public async Task<IActionResult> NewReleases()
        {
            var newReleaseProducts = await _productsService.GetAllProductsNewReleasesAsync();

            var model = new ProductsGridViewModel
            {
                Title = "New Releases",
                Products = newReleaseProducts
                .Select(p => new ProductCardViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price ?? 0,
                    Discount = p.Discount,
                    Quantity = p.Quantity,
                    Description = p.Description ?? "N/A",
                    ImagePath = p.ImagePath,
                    Variants = p.VariantDTOs.Select(v => new ProductVariantViewModel
                    {
                        Id = v.Id,
                        Color = v.ColorName,
                        ColorId = v.ColorId,
                        ColorHex = v.ColorHex,
                        SizeCode = v.SizeCode,
                        SizeId = v.SizeId,
                        Quantity = v.Quantity
                    })
                    .ToList(),
                }).ToList()
                //CarouselItems = await _carouselService.GetCarouselItemsAsync() // if you want the same carousel
            };

            return View(model);
        }

        // GET: ProductsController/HotDeals
        public async Task<IActionResult> HotDeals()
        {
            var hotDealsProducts = await _productsService.GetAllProductsHotDealsAsync();

            var model = new ProductsGridViewModel
            {
                Title = "Hot Deals",
                Products = hotDealsProducts
                .Select(p => new ProductCardViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price ?? 0,
                    Discount = p.Discount,
                    Quantity = p.Quantity,
                    Description = p.Description,
                    ImagePath = p.ImagePath,
                    Variants = p.VariantDTOs.Select(v => new ProductVariantViewModel
                    {
                        Id = v.Id,
                        Color = v.ColorName,
                        ColorId = v.ColorId,
                        ColorHex = v.ColorHex,
                        SizeCode = v.SizeCode,
                        SizeId = v.SizeId,
                        Quantity = v.Quantity
                    }).ToList(),
                }).ToList()
            };

            return View(model);
        }

        // GET: ProductsController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            ProductDTO? productDTO = await _productsService.GetProductByIdAsync(id);

            if (productDTO == null)
            {
                return NotFound();
            }

            ProductViewModel productVM = new()
            {
                Id = productDTO.Id,
                Name = productDTO.Name,
                CategoryId = productDTO.CategoryId,
                CategoryName = productDTO.CategoryName,
                Description = productDTO.Description ?? "N/A",
                Price = productDTO.Price ?? 0m,
                Discount = productDTO.Discount,
                Quantity = productDTO.Quantity,
                Barcode = productDTO.Barcode,
                ImagePath = productDTO.ImagePath,
                Variants = productDTO.VariantDTOs.Select(v => new ProductVariantViewModel
                {
                    Id = v.Id,
                    Color = v.ColorName,
                    ColorId = v.ColorId,
                    ColorHex = v.ColorHex,
                    SizeId = v.SizeId,
                    SizeCode = v.SizeCode,
                    Quantity = v.Quantity
                }).ToList()
            };

            return View(productVM);
        }

        // GET: ProductsController/GetProductVariants/5
        public async Task<IActionResult> GetProductVariants(int id)
        {
            var productVariantsDTO = await _productsService.GetAllProductVariantsAsync(id);

            var safeVariants = productVariantsDTO.Select(v => new
            {
                v.Id,
                v.ColorId,
                v.SizeId,
                v.Quantity
            });

            return Ok(safeVariants);
        }

        // GET: ProductsController/GetProductVariant/{key}
        public async Task<IActionResult> GetProductVariantIdByKey(string key)
        {
            var productVariantDTO = await _productsService.GetProductVariantByKeyAsync(key);

            if (productVariantDTO == null)
            {
                return Ok(new { success = false, message = "Product variant not found." });
            }

            return Ok(new { success = true, variantId = productVariantDTO.Id });
        }
    }
}
