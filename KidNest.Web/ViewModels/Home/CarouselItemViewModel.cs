using KidNest.Core.Enums;

namespace KidNest.Web.ViewModels.Home
{
    public class CarouselItemViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public ContentType? Type { get; set; }
        public bool IsActive { get; set; }
    }
}
