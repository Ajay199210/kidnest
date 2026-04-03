using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.ViewComponents
{
    public class AdsMenuViewComponent : ViewComponent
    {
        public AdsMenuViewComponent() {}

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return await Task.FromResult(View());
        }
    }
}
