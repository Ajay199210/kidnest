using KidNest.Core.Enums;
using System.ComponentModel;

namespace KidNest.Web.ViewModels.Contents
{
    public class ContentIndexViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ContentType? Type { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }
    }
}
