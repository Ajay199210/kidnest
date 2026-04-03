using KidNest.Web.ViewModels.Products;

namespace KidNest.Web.ViewModels.Home
{
    public class IndexPageViewModel
    {
        public List<CarouselItemViewModel> CarouselItems { get; set; } = [];
        public List<CategorySectionViewModel> Categories { get; set; } = [];

        // For search results
        public ProductsGridViewModel? ProductsGrid { get; set; } = null;
    }
}
