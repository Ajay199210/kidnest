using KidNest.Services.DTOs.Products;
using KidNest.Services.DTOs.ShoppingCart;
using KidNest.Services.Extensions;
using KidNest.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace KidNest.Services.Services
{
    public class CartService : ICartService
    {
        private const string CartKey = "Cart";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductsService _productsService;

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public CartService(IHttpContextAccessor httpContextAccessor, IProductsService productsService)
        {
            _httpContextAccessor = httpContextAccessor;
            _productsService = productsService;
        }

        public async Task AddToCartAsync(CartItemDTO itemDTO)
        {
            var cart = GetCart();

            var productDTO = await _productsService.GetProductByIdAsync(itemDTO.ProductId);
            if (productDTO == null)
                throw new InvalidOperationException("Product does not exist.");

            // Try to find matching cart item
            var existingCartItem = cart.FirstOrDefault(p =>
                p.ProductId == itemDTO.ProductId &&
                p.ColorId == itemDTO.ColorId &&
                p.SizeId == itemDTO.SizeId
            );

            int newTotalQuantity = existingCartItem != null
                ? existingCartItem.Quantity + itemDTO.Quantity
                : itemDTO.Quantity;

            // Check if the product has variants
            if (productDTO.VariantDTOs.Count > 0)
            {
                // Try to match the variant
                var variantDTO = productDTO.VariantDTOs.FirstOrDefault(v =>
                    (!v.ColorId.HasValue || v.ColorId == itemDTO.ColorId) &&
                    (!v.SizeId.HasValue || v.SizeId == itemDTO.SizeId)
                );

                if (variantDTO == null)
                    throw new InvalidOperationException("Selected variant does not exist.");

                // Ensure we do not exceed variant stock
                int variantAlreadyInCart = existingCartItem?.Quantity ?? 0;
                if (variantDTO.Quantity < newTotalQuantity)
                {
                    int remaining = variantDTO.Quantity - variantAlreadyInCart;
                    if (remaining > 0)
                    {
                        throw new InvalidOperationException($"Cannot add more than available variant stock. " +
                            $"({remaining} left)");
                    }

                    throw new InvalidOperationException("Cannot add more units of this item.");
                }
            }
            else
            {
                // For products without variants, check global stock
                int alreadyInCart = cart
                    .Where(p => p.ProductId == itemDTO.ProductId)
                    .Sum(p => p.Quantity);

                int totalRequested = alreadyInCart + itemDTO.Quantity;

                if (totalRequested > productDTO.Quantity)
                {
                    int remaining = productDTO.Quantity - alreadyInCart;
                    throw new InvalidOperationException($"Cannot add more than available stock. ({remaining} left)");
                }
            }

            if (existingCartItem != null)
            {
                existingCartItem.Quantity = newTotalQuantity;
            }
            else
            {
                cart.Add(itemDTO);
            }

            Session.SetObjectAsJson(CartKey, cart);
        }

        public void UpdateQuantity(int productId, int quantity, int? colorId, int? sizeId)
        {
            var cart = GetCart();  // Retrieve the cart

            // Find the item in the cart
            var existingCartItem = cart.FirstOrDefault(p =>
                p.ProductId == productId &&
                p.ColorId == colorId &&
                p.SizeId == sizeId
            //(p.ColorId == colorId || (!p.ColorId.HasValue && !colorId.HasValue)) &&
            //(p.SizeId == sizeId || (!p.SizeId.HasValue && !sizeId.HasValue))
            );

            if (existingCartItem != null)
            {
                // Update the quantity
                existingCartItem.Quantity = quantity;
            }
            else
            {
                throw new InvalidOperationException("Item not found in the cart.");
            }

            // Save the updated cart back
            Session.SetObjectAsJson(CartKey, cart);
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var itemToRemove = cart.FirstOrDefault(p => p.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove); // Remove the item
                Session.SetObjectAsJson(CartKey, cart); // Persist changes
            }
        }

        public List<CartItemDTO> GetCart()
        {
            List<CartItemDTO> cartItems = Session.GetObjectFromJson<List<CartItemDTO>>(CartKey) ?? [];

            return cartItems;
        }

        public int GetTotalItemCount()
        {
            var cart = GetCart();

            return cart.Sum(x => x.Quantity);
        }

        public void ClearCart()
        {
            Session.Remove(CartKey);
        }
    }
}
