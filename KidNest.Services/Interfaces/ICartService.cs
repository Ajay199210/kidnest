using KidNest.Services.DTOs.ShoppingCart;

namespace KidNest.Services.Interfaces
{
    public interface ICartService
    {
        Task AddToCartAsync(CartItemDTO item);
        void UpdateQuantity(int productId, int quantity, int? colorId, int? sizeId);
        void RemoveFromCart(int productId);
        List<CartItemDTO> GetCart();
        int GetTotalItemCount();
        void ClearCart();
    }
}
