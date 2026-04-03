namespace KidNest.Core.Entities
{
    public class Product : BaseEntity
    {
        public int CategoryId { get; set; }
        public string? Barcode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public int Quantity { get; set; }
        public string? ImagePath {  get; set; }
        public bool? IsNewRelease { get; set; }
        public DateTime? NewReleaseUntil { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        //public List<MdColor> ProductColors { get; set; } = [];
        //public List<MdSize> ProductSizes { get; set; } = [];
        public List<ProductVariant> ProductVariants { get; set; } = [];
    }
}
