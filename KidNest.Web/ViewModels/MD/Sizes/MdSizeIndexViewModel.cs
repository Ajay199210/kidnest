using System.ComponentModel;

namespace KidNest.Web.ViewModels.MD.Sizes
{
    public class MdSizeIndexViewModel
    {
        public int Id { get; set; }

        [DisplayName("Size Code")]
        public string? SizeCode { get; set; }
        public string? Description { get; set; }

        [DisplayName("Is Active")]
        public bool? IsActive { get; set; }

        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }
    }
}
