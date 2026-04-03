using KidNest.Web.ViewModels.Home;

namespace KidNest.Web.ViewModels.Products
{
    public class ProductsGridViewModel
    {
        public string? Title { get; set; }
        public List<ProductCardViewModel> Products { get; set; } = [];
    }
}
