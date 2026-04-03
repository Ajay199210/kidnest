using Microsoft.AspNetCore.Mvc.Rendering;

namespace KidNest.Web.Models
{
    public interface IProductVariantDropdowns
    {
        IEnumerable<SelectListItem> Colors { get; set; }
        IEnumerable<SelectListItem> Sizes { get; set; }
    }
}
