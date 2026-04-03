namespace KidNest.Web.ViewModels.Home
{
    public class CategorySectionViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public string ShortDescription
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Description))
                    return "N/A";

                var span = Description.AsSpan();

                return span.Length > 105 ? $"{span[..100]}..." : Description;
            }
        }

        public List<ProductCardViewModel> Products { get; set; } = [];
    }
}
