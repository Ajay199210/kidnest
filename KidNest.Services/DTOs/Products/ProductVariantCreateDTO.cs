using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Services.DTOs.Products
{
    public class ProductVariantCreateDTO
    {
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }
        public int CategoryId { get; set; }
        public int Quantity { get; set; }
        //public string? Barcode { get; set; }

        // Maybe add later
        //public bool IsActive { get; set; }
        //public DateTime? CreatedDate { get; set; }
        //public DateTime? ModifiedDate { get; set; }
    }
}
