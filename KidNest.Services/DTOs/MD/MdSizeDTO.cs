using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidNest.Services.DTOs.MD
{
    public class MdSizeDTO
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? SizeCode { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
