using KidNest.Core.Enums;

namespace KidNest.Services.DTOs.Contents
{
    public class ContentDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ContentType? Type { get; set; }
        public string? Path { get; set; }
        public bool IsActive { get; set; }
    }
}
