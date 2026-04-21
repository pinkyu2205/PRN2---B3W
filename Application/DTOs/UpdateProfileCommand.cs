using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateProfileCommand
    {
        [Required]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;

        [Url]
        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        public string ModifiedBy { get; set; } = "user";
    }
}
