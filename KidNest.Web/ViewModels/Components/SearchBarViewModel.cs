namespace KidNest.Web.ViewModels.Components
{
    public class SearchBarViewModel
    {
        public List<CategoryDropdownItem> Categories { get; set; } = [];
        public string? SearchTerm { get; set; }
        public int? SelectedCategoryId { get; set; }
    }

    public class CategoryDropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
