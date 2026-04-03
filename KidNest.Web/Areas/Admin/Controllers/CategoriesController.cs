using KidNest.Core.Shared;
using KidNest.Services.DTOs.Categories;
using KidNest.Services.Interfaces;
using KidNest.Web.Hubs;
using KidNest.Web.ViewModels.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace KidNest.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(AuthenticationSchemes = "AdminScheme")]
    //[Authorize(AuthenticationSchemes = "AdminScheme"), Roles = "Admin]
    [Authorize(Policy = "AdminOnly")] // can replace the above
    public class CategoriesController : Controller
    {
        private readonly ICategoriesService _categoriesService;

        public CategoriesController(ICategoriesService categoriesService, IHubContext<StoreHub> hubContext)
        {
            _categoriesService = categoriesService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoriesService.GetAllCategoriesAsync();

            var categoriesIndexVM = categories.Select(c => new CategoryIndexViewModel()
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            });

            return View(categoriesIndexVM);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel categoryCreateVM)
        {
            if (!ModelState.IsValid)
            {
                return View(categoryCreateVM);
            }

            var categoryCreateDTO = new CategoryCreateDTO
            {
                Name = categoryCreateVM.Name,
                Description = categoryCreateVM.Description,
            };

            var result = await _categoriesService.CreateCategoryAsync(categoryCreateDTO);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message!);

                return View(categoryCreateVM);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            CategoryDTO? categoryDTO = await _categoriesService.GetCategoryByIdAsync(id);

            if (categoryDTO == null)
            {
                return NotFound();
            }

            CategoryViewModel categoryVM = new()
            {
                Id = categoryDTO.Id,
                Name = categoryDTO.Name,
                Description = categoryDTO.Description
            };

            return View(categoryVM);
        }

        public async Task<IActionResult> Edit(int id)
        {
            CategoryDTO? categoryDTO = await _categoriesService.GetCategoryByIdAsync(id);

            if (categoryDTO == null)
            {
                return NotFound();
            }

            // Map from general category view model to the category edit view model
            CategoryEditViewModel categoryToEditVM = new()
            {
                Id = categoryDTO.Id,
                Name = categoryDTO.Name,
                Description = categoryDTO.Description,
            };

            return View(categoryToEditVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryEditViewModel categoryEditVM)
        {
            if(id != categoryEditVM.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(categoryEditVM);
            }

            var categoryToEdit = new CategoryDTO
            {
                Id = categoryEditVM.Id,
                Name = categoryEditVM.Name,
                Description = categoryEditVM.Description
            };

            var result = await _categoriesService.UpdateCategoryAsync(categoryToEdit);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message!);

                return View(categoryEditVM);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _categoriesService.GetCategoryByIdAsync(id);

                if (category == null)
                    return NotFound();
                
                CategoryViewModel categoryVM = new()
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };

                return View(categoryVM);
            }
            catch (Exception ex)
            {
                // Log error (ex, "Error fetching product")
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CategoryViewModel categoryVM)
        {
            try
            {
                OperationResult deleteResult = await _categoriesService.DeleteCategoryAsync(id);

                if (deleteResult.Success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Add each error individually to ModelState
                    foreach (var error in deleteResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }

                    // Optionally, if you want to also use the Message:
                    if (!string.IsNullOrEmpty(deleteResult.Message))
                    {
                        ModelState.AddModelError(string.Empty, deleteResult.Message);
                    }

                    return View(categoryVM);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
