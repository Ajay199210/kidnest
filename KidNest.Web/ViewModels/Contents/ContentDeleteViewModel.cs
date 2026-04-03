using System.ComponentModel;

namespace KidNest.Web.ViewModels.Contents
{
    public class ContentDeleteViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? Type { get; set; } // an enum might be better

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }
    }
}
