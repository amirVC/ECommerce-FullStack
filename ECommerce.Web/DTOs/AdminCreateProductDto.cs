using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.DTOs;
public class AdminCreateProductDto
{
    [Required(ErrorMessage = "SKU is required.")]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageAltText { get; set; }
    public int CategoryId { get; set; }

    // Sale pricing
    public decimal? SalePrice { get; set; }
    public DateTime? SaleStartDate { get; set; }
    public DateTime? SaleEndDate { get; set; }
}