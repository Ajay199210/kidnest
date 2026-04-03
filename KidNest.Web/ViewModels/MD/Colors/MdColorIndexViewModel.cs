using System.ComponentModel;

namespace KidNest.Web.ViewModels.MD.Colors
{
    public class MdColorIndexViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        [DisplayName("Is Active")]
        public bool? IsActive { get; set; }

        [DisplayName("Created Date")]
        public DateTime? CreatedDate { get; set; }
    }
}
