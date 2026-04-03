using KidNest.Services.DTOs.Orders;
using System.ComponentModel;

namespace KidNest.Web.ViewModels.Orders
{
    public class OrderIndexViewModel
    {
        public int Id { get; set; }

        [DisplayName("Order Date")]
        public DateTime? OrderDate { get; set; }

        [DisplayName("User Email")]
        public string? UserEmail { get; set; }

        public string? Status { get; set; }

        //[DisplayName("Items Count")]
        //public int OrderItemsCount { get; set; }

        public List<OrderItemViewModel> OrderItems { get; set; } = [];

        // Calculated
        [DisplayName("Items Count")]
        public int OrderItemsCount => OrderItems.Sum(oi => oi.Quantity);

        [DisplayName("Order Total Price")]
        public decimal OrderTotalPrice => OrderItems.Sum(oi => oi.TotalPrice);

        // Formatting helper
        //public string FormattedDate => OrderDate!.ToString("ddMMMyyyy");
    }
}
