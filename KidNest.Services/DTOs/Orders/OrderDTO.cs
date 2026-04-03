using KidNest.Core.Enums;

namespace KidNest.Services.DTOs.Orders
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public int? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? UserAddress { get; set; }
        public string? Status { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }

        public List<OrderItemDTO> OrderItemsDTO { get; set; } = [];

        // Calculated
        public int TotalItemCount => OrderItemsDTO.Sum(oi => oi.Quantity);
        public decimal TotalOrderPrice => OrderItemsDTO.Sum(oi => oi.DiscountedPrice * oi.Quantity);
    }
}
