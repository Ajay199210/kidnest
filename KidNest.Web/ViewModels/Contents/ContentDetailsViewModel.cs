using KidNest.Core.Enums;
using System.ComponentModel;

namespace KidNest.Web.ViewModels.Contents
{
    public class ContentDetailsViewModel
    {
        public int Id { get; set; }
        
        public string? Name { get; set; }
        
        public ContentType? Type { get; set; }

        public string? Path { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }
    }
}
