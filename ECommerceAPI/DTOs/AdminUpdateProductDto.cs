using System.ComponentModel.DataAnnotations;

namespace ECommerceAPI.DTOs;

public class AdminUpdateProductDto
{
    [Required]
    [MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Range(0, 100000)]
    public int Stock { get; set; }

    public string? ImageUrl { get; set; }

    public IFormFile? ImageFile { get; set; }

    public string? ImageAltText { get; set; }

    [Required]
    public int CategoryId { get; set; }

    // Sale pricing
    [Range(0.01, 1000000)]
    public decimal? SalePrice { get; set; }

    public DateTime? SaleStartDate { get; set; }

    public DateTime? SaleEndDate { get; set; }
}