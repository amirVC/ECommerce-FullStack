namespace ECommerceAPI.DTOs
{
    public class UpdateReviewDto
    {
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}