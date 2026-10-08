namespace ECommerce.Web.DTOs;

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string? ProductThumbnailUrl { get; set; }   

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    public int AvailableStock { get; set; }

    public bool IsOnSale { get; set; }
    public decimal OriginalUnitPrice { get; set; }
}