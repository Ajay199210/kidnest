namespace KidNest.Web.ViewModels.Components
{
    public class CartItemUpdateQuantityViewModel
    {
        public int ProductId { get; set; }
        //public int? VariantId { get; set; }
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }
        public int Quantity { get; set; }
    }
}
