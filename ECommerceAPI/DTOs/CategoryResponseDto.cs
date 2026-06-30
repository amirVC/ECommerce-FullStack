namespace ECommerceAPI.DTOs
{
    public class CategoryResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<ProductSummaryDto> Products { get; set; } = new();
    }
}