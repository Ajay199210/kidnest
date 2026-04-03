using KidNest.Core.Enums;

namespace KidNest.Services.DTOs.Orders
{
    public class OrderCreateDTO
    {
        public DateTime? OrderDate { get; set; }
        public int UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }

        public List<OrderItemDTO>? OrderItemsDTO { get; set; }

        public OrderStatus OrderStatus { get; set; }
    }
}
