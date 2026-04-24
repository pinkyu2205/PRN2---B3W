using Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Notification : BaseFullEntity
    {
        public int? AccountId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Type { get; set; } = "info"; // info, success, warning, danger

        public bool IsRead { get; set; } = false;

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }
    }
}
