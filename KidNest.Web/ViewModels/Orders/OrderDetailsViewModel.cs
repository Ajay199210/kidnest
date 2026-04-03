using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.ViewModels.Orders
{
    public class OrderDetailsViewModel
    {
        public int Id { get; set; }

        [DisplayName("Order Date")]
        public DateTime? OrderDate { get; set; }

        [DisplayName("User Email")]
        public string? UserEmail { get; set; }

        [DisplayName("User Address")]
        public string? UserAddress { get; set; }

        public string? Status { get; set; }

        [DisplayName("Modified Date")]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Modified By")]
        public string? ModifiedBy { get; set; }

        [DisplayName("Items Count")]
        public int TotalItemsCount { get; set; }

        [DisplayName("Total Price")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        public decimal TotalOrderPrice {  get; set; }

        
        [DisplayName("Ordered Items")]
        public List<OrderItemViewModel> ItemsList { get; set; } = [];
    }
}
