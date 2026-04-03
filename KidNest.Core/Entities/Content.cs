using KidNest.Core.Enums;

namespace KidNest.Core.Entities
{
    public class Content : BaseEntity
    {
        public string? Name { get; set; }
        public ContentType? Type { get; set; }
        public string? Path { get; set; }
        public bool IsActive { get; set; }
    }
}
