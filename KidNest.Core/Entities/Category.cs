namespace KidNest.Core.Entities
{
    public class Category : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        // Navigation property
        public ICollection<Product>? Products { get; set; }
    }
}
