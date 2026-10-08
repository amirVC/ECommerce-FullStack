namespace ECommerceAPI.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }   
    public bool IsVerifiedPurchase { get; set; }
    public DateTime CreatedAt { get; set; }
}