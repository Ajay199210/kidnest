namespace KidNest.Core.Entities
{
    public class Order : BaseEntity
    {
        public DateTime? OrderDate { get; set; }
        public int? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }

        // Navigation Properties
        public AppUser? User { get; set; }
        public List<OrderItem> OrderItems { get; set; } = [];
    }
}
