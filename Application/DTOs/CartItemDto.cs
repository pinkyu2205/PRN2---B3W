namespace Application.DTOs
{
    public class CartItemDTO
    {
        public ProductDTO Product { get; set; } = new ProductDTO();
        public int Quantity { get; set; }
        public decimal TotalPrice => Product.Price * Quantity;
    }
}

