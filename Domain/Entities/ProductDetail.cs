using Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("ProductDetails")]
    public partial class ProductDetail : BaseFullEntity
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập màu sắc.")]
        [StringLength(255, ErrorMessage = "Màu sắc tối đa 255 ký tự.")]
        public string? Color { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập kích thước.")]
        [StringLength(255, ErrorMessage = "Kích thước tối đa 255 ký tự.")]
        public string? Size { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập chất liệu.")]
        [StringLength(255, ErrorMessage = "Chất liệu tối đa 255 ký tự.")]
        public string? Material { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập xuất xứ.")]
        [StringLength(255, ErrorMessage = "Xuất xứ tối đa 255 ký tự.")]
        public string? Origin { get; set; }

        public string? ImageUrl { get; set; }
        
        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
        public string? Description { get; set; }
        
        [Range(0, 999999999, ErrorMessage = "Giá phụ thêm không hợp lệ.")]
        public decimal? AdditionalPrice { get; set; }

        public virtual Product? Product { get; set; }
    }

}
