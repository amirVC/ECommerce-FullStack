namespace ECommerce.Web.ViewModels;
public class CartItemViewModel
{
    public int Id { get; set; }           
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }           
    public decimal OriginalPrice { get; set; }    
    public bool IsOnSale { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }   
    public int AvailableStock { get; set; }
    public decimal Total => Price * Quantity;
}