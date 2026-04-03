using KidNest.Core.Entities;
using KidNest.Core.Shared;
using KidNest.Services.DTOs.Products;
using KidNest.Services.Extensions;
using KidNest.Services.Interfaces;
using KidNest.Services.Services;
using KidNest.Web.Hubs;
using KidNest.Web.Models;
using KidNest.Web.ViewModels.MD.Colors;
using KidNest.Web.ViewModels.MD.Sizes;
using KidNest.Web.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KidNest.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(AuthenticationSchemes = "AdminScheme")]
    //[Authorize(AuthenticationSchemes = "AdminScheme"), Roles = "Admin]
    [Authorize(Policy = "AdminOnly")] // can replace the above
    public class ProductsController : Controller
    {
        private readonly IProductsService _productsService;
        private readonly ICategoriesService _categoriesService;
        private readonly IHubContext<StoreHub> _hubContext;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMdColorsService _mdColorsService;
        private readonly IMdSizesService _mdSizesService;

        public ProductsController(IProductsService productsService, ICategoriesService categoriesService,
            IHubContext<StoreHub> hubContext, IFileStorageService fileStorageService, 
            IMdColorsService mdColorsService, IMdSizesService mdSizesService)
        {
            _productsService = productsService;
            _categoriesService = categoriesService;
            _mdColorsService = mdColorsService;
            _hubContext = hubContext;
            _fileStorageService = fileStorageService;
            _mdSizesService = mdSizesService;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductsData([FromBody] DataTableRequest request)
        {
            try
            {
                var result = await _productsService.GetPaginatedProductsAsync(request);

                return Json(new
                {
                    draw = request.Draw,
                    recordsTotal = result.TotalCount,
                    recordsFiltered = result.TotalCount, // Same if no separate filtering
                    data = result.Items
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Server error processing your request", details = ex.Message });
            }
        }

        // GET: ProductsController
        //public async Task<IActionResult> Index()
        public IActionResult Index()
        {
            //var products = await _productsService.GetAllProductsAsync();

            //var productsIndexVM = products.Select(p => new ProductIndexViewModel()
            //{
            //    Id = p.Id,
            //    Name = p.Name,
            //    Description = p.Description,
            //    CategoryName = p.CategoryName,
            //    Price = p.Price,
            //    Discount = p.Discount,
            //    Quantity = p.Quantity,
            //    Barcode = p.Barcode,
            //});

            //return View(productsIndexVM);

            return View();
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
                CategoryName = productDTO.CategoryName,
                Description = productDTO.Description,
                Price = productDTO.Price,
                Discount = productDTO.Discount,
                Quantity = productDTO.Quantity,
                Barcode = productDTO.Barcode,
                ImagePath = productDTO.ImagePath,
                NewReleaseUntil = productDTO.NewReleaseUntil,
                Variants = productDTO.VariantDTOs.Select(v => new ProductVariantViewModel
                {
                    Id = v.Id,
                    Color = v.ColorName,
                    ColorHex = v.ColorHex,
                    SizeCode = v.SizeCode,
                    Barcode = v.Barcode,
                    Quantity = v.Quantity
                }).ToList(),
            };

            return View(productVM);
        }

        // GET: ProductsController/Create
        public async Task<IActionResult> Create()
        {
            ProductCreateViewModel productCreateVM = new()
            {
                Categories = await _categoriesService.GetCategoriesSelectListAsync(),
            };
            
            await PopulateSelectListsAsync(productCreateVM);

            return View(productCreateVM);
        }

        // POST: ProductsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel productCreateVM)
        {
            if (!ModelState.IsValid)
            {
                productCreateVM.Categories = await _categoriesService.GetCategoriesSelectListAsync();
                await PopulateSelectListsAsync(productCreateVM);

                return View(productCreateVM);
            }

            try
            {
                ProductCreateDTO productCreateDTO = new()
                {
                    Name = productCreateVM.Name,
                    Description = productCreateVM.Description,
                    Barcode = productCreateVM.Barcode,
                    CategoryId = productCreateVM.CategoryId,
                    Price = productCreateVM.Price,
                    Discount = productCreateVM.Discount,
                    Quantity = productCreateVM.Quantity,
                    IsNewRelease = productCreateVM.IsNewRelease,
                    SelectedColorIds = productCreateVM.SelectedColorIds,
                    SelectedSizeIds = productCreateVM.SelectedSizeIds,
                    VariantCreateDTOs = productCreateVM.Variants.Select(v => new ProductVariantCreateDTO
                    {
                        ColorId = v.ColorId,
                        SizeId = v.SizeId,
                        Quantity = v.Quantity
                    }).ToList()
                };

                // Save product image
                if (productCreateVM.ImageFile != null)
                {
                    string? imagePath = await _fileStorageService.SaveFileAsync(productCreateVM.ImageFile,
                        "uploads/img/products");

                    productCreateDTO.ImagePath = imagePath;
                }

                var result = await _productsService.CreateProductAsync(productCreateDTO);

                if (!result.Success)
                {
                   ModelState.AddErrors(result);

                    productCreateVM.Categories = await _categoriesService.GetCategoriesSelectListAsync();
                    await PopulateSelectListsAsync(productCreateVM);

                    return View(productCreateVM);
                }

                //await _hubContext.Clients.All.SendAsync("ProductAdded", productCreateDTO);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: ProductsController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            ProductDTO? productDTO = await _productsService.GetProductByIdAsync(id);

            if (productDTO == null)
            {
                return NotFound();
            }

            // Map from general product view model to the product edit view model
            ProductEditViewModel productEditVM = new()
            {
                Id = productDTO.Id,
                Name = productDTO.Name,
                Description = productDTO.Description,
                CategoryId = productDTO.CategoryId,
                Price = productDTO.Price,
                Quantity = productDTO.Quantity,
                Discount = productDTO.Discount,
                Barcode = productDTO.Barcode,
                ImagePath = productDTO.ImagePath,
                Categories = await _categoriesService.GetCategoriesSelectListAsync(productDTO.CategoryId),

                // Extract distinct color IDs from variants
                SelectedColorIds = productDTO.VariantDTOs
                    .Select(v => v.ColorId)
                    .OfType<int>()
                    .Distinct()
                    .ToList(),

                // Extract distinct size IDs from variants
                SelectedSizeIds = productDTO.VariantDTOs
                     .Select(v => v.SizeId)
                    .OfType<int>()
                    .Distinct()
                    .ToList(),

                // Variants
                Variants = productDTO.VariantDTOs.Select(v => new ProductVariantInputModel
                {
                    ColorId = v.ColorId,
                    SizeId = v.SizeId,
                    Quantity = v.Quantity
                }).ToList(),

                IsNewRelease = Convert.ToBoolean(productDTO.IsNewRelease),
                NewReleaseUntil = productDTO.NewReleaseUntil
            };

            await PopulateSelectListsAsync(productEditVM);

            return View(productEditVM);
        }

        // POST: ProductsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel productEditVM)
        {
            if (!ModelState.IsValid)
            {
                productEditVM.Categories = await _categoriesService.GetCategoriesSelectListAsync(id);
                await PopulateSelectListsAsync(productEditVM);

                return View(productEditVM);
            }

            if (id != productEditVM.Id)
            {
                return NotFound();
            }

            try
            {
                ProductDTO productDTO = new()
                {
                    Id = productEditVM.Id,
                    Name = productEditVM.Name,
                    Description = productEditVM.Description,
                    CategoryId = productEditVM.CategoryId,
                    Price = productEditVM.Price,
                    Quantity = productEditVM.Quantity,
                    Discount = productEditVM.Discount,
                    Barcode = productEditVM.Barcode,
                    ImagePath = productEditVM.ImagePath,
                    SelectedColorIds = productEditVM.SelectedColorIds,
                    SelectedSizeIds = productEditVM.SelectedSizeIds,
                    IsNewRelease = productEditVM.IsNewRelease,
                    NewReleaseUntil = productEditVM.NewReleaseUntil,
                    VariantDTOs = productEditVM.Variants.Select(v => new ProductVariantDTO
                    {
                        ColorId = v.ColorId,
                        SizeId = v.SizeId,
                        Quantity = v.Quantity
                    }).ToList()
                };

                // Save product image
                if (productEditVM.ImageFile != null)
                {
                    string? imagePath = await _fileStorageService.SaveFileAsync(productEditVM.ImageFile, "uploads/img/products");
                    productDTO.ImagePath = imagePath;
                }

                //try
                //{
                var result = await _productsService.UpdateProductAsync(productDTO);

                if (!result.Success)
                {
                    ModelState.AddErrors(result);

                    productEditVM.Categories = await _categoriesService.GetCategoriesSelectListAsync(id);
                    await PopulateSelectListsAsync(productEditVM);

                    return View(productEditVM);
                }

                // Notify clients via SignalR
                //await _hubContext.Clients.All.SendAsync("ProductUpdated", productDTO);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return StatusCode(500, $"Internal server error");
            }
        }

        // GET: ProductsController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var productDTO = await _productsService.GetProductByIdAsync(id);

                if (productDTO == null)
                    return NotFound();

                ProductViewModel productVM = new()
                {
                    Id = productDTO.Id,
                    Name = productDTO.Name,
                    CategoryName = productDTO.CategoryName,
                    Description = productDTO.Description,
                    Price = productDTO.Price,
                    Discount = productDTO.Discount,
                    Quantity = productDTO.Quantity,
                    Barcode = productDTO.Barcode,
                    ImagePath = productDTO.ImagePath,
                    Variants = productDTO.VariantDTOs.Select(v => new ProductVariantViewModel
                    {
                        Id = v.Id,
                        Color = v.ColorName,
                        ColorHex = v.ColorHex,
                        SizeCode = v.SizeCode,
                        Barcode = "",
                        Quantity = v.Quantity
                    }).ToList(),
                };

                return View(productVM);
            }
            catch
            {
                // Log error (ex, "Error fetching product")
                return StatusCode(500, $"Internal server error");
            }
        }

        // POST: ProductsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, ProductViewModel productVM)
        {
            if (id != productVM.Id)
            {
                return NotFound();
            }

            try
            {
                OperationResult deleteResult = await _productsService.DeleteProductAsync(id);

                if (deleteResult.Success)
                    return RedirectToAction(nameof(Index));
                else
                {
                    ModelState.AddModelError("", deleteResult.Message!);
                    return View(productVM);
                }
            }
            catch
            {
                return StatusCode(500, $"Internal server error");
            }
        }

        private async Task PopulateSelectListsAsync(IProductVariantDropdowns model)
        {
            model.Colors = await _mdColorsService.GetMdColorsSelectListAsync();
            model.Sizes = await _mdSizesService.GetMdSizesSelectListAsync();
        }
    }
}
