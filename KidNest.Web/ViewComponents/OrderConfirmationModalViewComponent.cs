using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.Orders;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.ViewComponents
{
    public class OrderConfirmationModalViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public OrderConfirmationModalViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()

        {
            var cartItems = await Task.FromResult(_cartService.GetCart());
            var viewModel = new OrderCreateViewModel();

            viewModel.OrderItems = cartItems.Select(item => new OrderItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                Price = item.ProductPrice,
            }).ToList();

            return View(viewModel);
        }
    }
}
