using KidNest.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.ViewComponents
{
    public class CartOffcanvasViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartOffcanvasViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cartItems = await Task.FromResult(_cartService.GetCart());

            return View(cartItems);
        }
    }
}
