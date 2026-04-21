using Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Products")]
    public partial class Product : BaseFullEntity
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tên sản phẩm tối đa 255 ký tự.")]
        public string ProductName { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
        public string? Description { get; set; }

        [Range(0.01, 999999999, ErrorMessage = "Giá phải lớn hơn 0.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng không hợp lệ.")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; }
        
        [StringLength(255)]
        public string CategoryName { get; set; } = string.Empty;
        // Navigation can be null during create/edit posts; FK enforces requiredness
        public virtual Category? Category { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<ProductDetail> ProductDetails { get; set; } = new List<ProductDetail>();
    }
}
