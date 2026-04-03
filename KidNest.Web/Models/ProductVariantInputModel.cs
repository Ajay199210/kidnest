using System.ComponentModel.DataAnnotations;

namespace KidNest.Web.Models
{
    public class ProductVariantInputModel
    {
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity can't have a negative value")]
        public int Quantity { get; set; }
    }
}
