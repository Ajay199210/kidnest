using KidNest.Web.ViewModels.Home;

namespace KidNest.Web.ViewModels.Categories
{
    public class CategoryProductsViewModel
    {
        public string? CategoryName { get; set; }
        public List<ProductCardViewModel> Products { get; set; } = [];
    }
}
