using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.Categories;
using KidNest.Web.ViewModels.Home;
using KidNest.Web.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.Controllers
{
    public class CategoriesController : Controller
    {
        IProductsService _productsService;

        public CategoriesController(IProductsService productsService)
        {
            _productsService = productsService;
        }

        // GET: CatalogController/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var categoryProductsDTO = await _productsService.GetProductsByCategoryIdAsync(id);

            if (categoryProductsDTO.ToList().Count == 0)
            {
                return NotFound();
            }

            CategoryProductsViewModel categoryProductsVM = new()
            {
                CategoryName = categoryProductsDTO.First().CategoryName
            };

            foreach (var productDTO in categoryProductsDTO)
            {
                categoryProductsVM.Products.Add(new ProductCardViewModel
                {
                    Id = productDTO.Id,
                    Name = productDTO.Name,
                    Price = productDTO.Price ?? 0,
                    Discount = productDTO.Discount,
                    Description = productDTO.Description,
                    Quantity = productDTO.Quantity,
                    ImagePath = productDTO.ImagePath,
                    Variants = productDTO.VariantDTOs.Select(v => new ProductVariantViewModel
                    {
                        Id = v.Id,
                        Color = v.ColorName,
                        ColorId = v.ColorId,
                        ColorHex = v.ColorHex,
                        SizeCode = v.SizeCode,
                        SizeId = v.SizeId,
                        Quantity = v.Quantity
                    }).ToList()
                });
            }
            
            return View(categoryProductsVM);
        }
    }
}
