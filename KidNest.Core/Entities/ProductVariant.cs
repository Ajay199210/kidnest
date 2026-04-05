namespace KidNest.Core.Entities
{
    public class ProductVariant : Product
    {
        public int ProductId { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHex { get; set; }
        public int? SizeId { get; set; }
        public string? SizeCode { get; set; }
        public bool IsActive { get; set; }

        public string Key
        {
            get
            {
                if (ProductId <= 0)
                    throw new InvalidOperationException("ProductId must be set before generating ProductVariantKey.");

                var key = ProductId.ToString();

                if (ColorId.HasValue)
                    key += $"C{ColorId.Value}";

                if (SizeId.HasValue)
                    key += $"S{SizeId.Value}";

                return key;
            }
        }
    }
}
