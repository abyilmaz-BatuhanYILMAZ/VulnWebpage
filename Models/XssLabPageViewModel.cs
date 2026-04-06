using System.ComponentModel.DataAnnotations;

namespace vulnerablewebpage.Models
{
    public sealed class XssLabPageViewModel
    {
        public string ReflectedInput { get; set; } = string.Empty;

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, ErrorMessage = "Title can be at most 150 characters.")]
        public string StoredTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        [StringLength(4000, ErrorMessage = "Content can be at most 4000 characters.")]
        public string StoredContent { get; set; } = string.Empty;

        public List<XssEntry> StoredEntries { get; set; } = new();

        public string DomInput { get; set; } = string.Empty;
    }
}