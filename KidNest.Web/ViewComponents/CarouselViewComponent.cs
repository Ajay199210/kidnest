using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.ViewComponents
{
    public class CarouselViewComponent : ViewComponent
    {
        private readonly IContentsService _contentsService;

        public CarouselViewComponent(IContentsService contentsService)
        {
            _contentsService = contentsService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get carousel items
            var contentDTOs = await _contentsService.GetAllContentsAsync();

            var carouselItems = contentDTOs
                .Where(c => c.IsActive)
                .Select(ci => new CarouselItemViewModel
                {
                    Id = ci.Id,
                    Name = ci.Name,
                    Path = ci.Path,
                    Type = ci.Type,
                    IsActive = ci.IsActive,
                }).ToList();

            return View(carouselItems);
        }
    }
}
