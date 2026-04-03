using System.ComponentModel;

namespace KidNest.Web.ViewModels.MD.Colors
{
    public class MdColorViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        [DisplayName("Color")]
        public string? HexValue { get; set; }

        [DisplayName("Is Active")]
        public bool? IsActive { get; set; }

        [DisplayName("Created Date")]
        public DateTime? CreateDate { get; set; }
    }
}
