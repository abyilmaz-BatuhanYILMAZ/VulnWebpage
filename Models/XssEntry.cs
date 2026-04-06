using System.ComponentModel.DataAnnotations;

namespace vulnerablewebpage.Models
{
    public sealed class XssEntry
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(4000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
    }
}