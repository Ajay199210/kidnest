namespace KidNest.Core.Entities
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal? ProductPrice { get; set; }
        public decimal? ProductDiscount { get; set; }
        public int? ProductVariantId { get; set; }
        public int Quantity { get; set; }

        // Navigation Properties
        public MdColor? MdColor { get; set; }
        public MdSize? MdSize { get; set; }
    }
}
