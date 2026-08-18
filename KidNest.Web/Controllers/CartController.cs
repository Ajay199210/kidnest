using KidNest.Services.DTOs.ShoppingCart;
using KidNest.Services.Interfaces;
using KidNest.Web.ViewModels.Components;
using Microsoft.AspNetCore.Mvc;

namespace KidNest.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            int itemCount = _cartService.GetTotalItemCount();

            return Json(new { itemCount });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([FromBody] CartItemViewModel cartItemVM)
        {
            try
            {
                var cartItemDTO = new CartItemDTO
                {
                    ProductId = cartItemVM.ProductId,
                    ProductName = cartItemVM.ProductName,
                    ProductImage = cartItemVM.ProductImage,
                    ProductPrice = cartItemVM.ProductPrice,
                    //VariantId = cartItemVM.VariantId,
                    ColorId = cartItemVM.ColorId,
                    Color = cartItemVM.Color,
                    SizeId = cartItemVM.SizeId,
                    Size = cartItemVM.Size,
                    Quantity = cartItemVM.Quantity,
                    StockQuantity = cartItemVM.StockQuantity,
                };

                await _cartService.AddToCartAsync(cartItemDTO);

                int count = _cartService.GetTotalItemCount();

                return Json(new { success = true, itemCount = count });
            }
            catch (InvalidOperationException ex) // custom, expected errors
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity([FromBody] CartItemUpdateQuantityViewModel cartItemUpdateQuantityVM)
        {
            try
            {
                if (cartItemUpdateQuantityVM.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Quantity must be greater than zero." });
                }

                _cartService.UpdateQuantity(cartItemUpdateQuantityVM.ProductId, cartItemUpdateQuantityVM.Quantity,
                    cartItemUpdateQuantityVM.ColorId, cartItemUpdateQuantityVM.SizeId);
            
                int count = _cartService.GetTotalItemCount();

                return Json(new { success = true, itemCount = count });
            }
            catch (InvalidOperationException ex) // custom, expected errors
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                // Log the technical error if needed, but show generic message
                return Json(new { success = false, message = "An unexpected error occurred. Please try again later." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart([FromBody] int productId)
        {
            _cartService.RemoveFromCart(productId);
            int count = _cartService.GetTotalItemCount();

            return Json(new { success = true, itemCount = count });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cartService.ClearCart();

            return Json(new { success = true });
        }

        
        // Rendering partial view for cart items list (inside the offcanvas)
        public IActionResult RenderCartItemsPartial()
        {
            var cartItems = _cartService.GetCart();

            return PartialView("_CartItemsPartial", cartItems);
        }

        // Rendering the cart offcanvas view component
        public IActionResult RenderCartOffcanvas()
        {
            return ViewComponent("CartOffcanvas");
        }
    }
}
