namespace Application.DTOs
{
    public class ProductDetailDTO
    {
        public int Id { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Material { get; set; }
        public string? Origin { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public decimal? AdditionalPrice { get; set; }
    }

}
