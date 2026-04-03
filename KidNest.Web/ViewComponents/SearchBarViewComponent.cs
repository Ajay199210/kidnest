using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.Components;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.ViewComponents
{
    public class SearchBarViewComponent : ViewComponent
    {
        private readonly ICategoriesService _categoriesService;

        public SearchBarViewComponent(ICategoriesService categoriesService)
        {
            _categoriesService = categoriesService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string? query = null, int? categoryId = null)
        {
            var categories = await _categoriesService.GetAllCategoriesAsync();

            var searchBarVM = new SearchBarViewModel
            {
                Categories = categories.Select(c => new CategoryDropdownItem
                {
                    Id = c.Id,
                    Name = c.Name!
                }).ToList(),
                SearchTerm = query,
                SelectedCategoryId = categoryId
            };

            return View(searchBarVM);
        }
    }
}
