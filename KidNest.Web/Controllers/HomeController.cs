using KidNest.Services.DTOs.Products;
using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels;
using KidNest.Web.ViewModels.Home;
using KidNest.Web.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace KidNest.Web.Controllers
{
    //[Authorize(AuthenticationSchemes = "UserScheme")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoriesService _categoriesService;
        private readonly IProductsService _productService;
        private readonly IContentsService _contentsService;
        private readonly ISettingsService _settingsService;

        public HomeController(ILogger<HomeController> logger, 
            ICategoriesService categoriesService, IProductsService productService,
            IContentsService contentsService, ISettingsService settingsService)
        {
            _logger = logger;
            _categoriesService = categoriesService;
            _productService = productService;
            _contentsService = contentsService;
            _settingsService = settingsService;
        }

        public async Task<IActionResult> Index(string? query, int? categoryId)
        {
            IndexPageViewModel indexPageVM = new();

            // Get carousel items
            var contentDTOs = await _contentsService.GetAllContentsAsync();
            indexPageVM.CarouselItems = contentDTOs
                .Where(c => c.IsActive)
                .Select(ci => new CarouselItemViewModel
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    Path = ci.Path,
                    Type = ci.Type,
                    IsActive = ci.IsActive,
                }).ToList();

            // Show default homepage category sections with no search criteria
            if (string.IsNullOrWhiteSpace(query) && categoryId == null)
            {
                // Get available categories
                var categoryDTOs = await _categoriesService.GetAllCategoriesAsync();
                
                foreach (var categoryDTO in categoryDTOs)
                {
                    var productsDTO = await _productService.GetProductsByCategoryIdAsync(categoryDTO.Id, 3);
                    if(productsDTO.ToList().Count > 0)
                    {
                        indexPageVM.Categories.Add(new CategorySectionViewModel
                        {
                            Id = categoryDTO.Id,
                            Name = categoryDTO.Name!,
                            Description = categoryDTO.Description,
                            Products = productsDTO
                                .OrderByDescending(p => p.Name)
                                .Take(4)  // limit to 3 for display
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
                        });
                    }
                }
            }
            // Else, show flat products grid for the search results
            else
            {
                IEnumerable<ProductDTO> matchedProductsDTO;

                if (categoryId != null && !string.IsNullOrWhiteSpace(query))
                {
                    // Search within a category
                    matchedProductsDTO = await _productService.SearchProductsByCategoryAndQueryAsync(
                        categoryId.Value, query);
                }
                else if (categoryId != null)
                {
                    // List all products of the selected category (see more)
                    matchedProductsDTO = await _productService.GetProductsByCategoryIdAsync(categoryId.Value);
                }
                else
                {
                    // Search all products
                    matchedProductsDTO = await _productService.SearchProductsByNameOrDescAsync(query!);
                }

                indexPageVM.ProductsGrid = new ProductsGridViewModel
                {
                    Title = "Search Results",
                    Products = matchedProductsDTO
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
                        }).ToList(),
                    }).ToList()
                };
            }

            return View(indexPageVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
