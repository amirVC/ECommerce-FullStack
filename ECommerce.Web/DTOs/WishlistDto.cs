namespace ECommerce.Web.DTOs;

public class WishlistDto
{
    public int Id { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
    public int TotalItems { get; set; }
}