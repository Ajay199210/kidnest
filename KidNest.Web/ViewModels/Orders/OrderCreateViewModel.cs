namespace KidNest.Web.ViewModels.Orders
{
    public class OrderCreateViewModel
    {
        public int UserId { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = [];
        public string? ShippingAddress { get; set; }
        public string? PaymentMethod { get; set; }

        public decimal TotalAmount => OrderItems!.Sum(i => i.Quantity * i.Price);
    }
}
