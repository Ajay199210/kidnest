using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Services.DTOs.Products
{
    public class ProductVariantDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorHex { get; set; }
        public int? SizeId { get; set; }
        public string? SizeCode { get; set; }
        public int CategoryId { get; set; }
        public string? Barcode { get; set; }
        public int Quantity { get; set; }
    }
}
