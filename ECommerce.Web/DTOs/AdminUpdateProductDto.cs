using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.DTOs;

public class AdminUpdateProductDto : IValidatableObject
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

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (SalePrice.HasValue && SalePrice.Value >= Price)
        {
            yield return new ValidationResult(
                "Sale price must be lower than the regular price.",
                new[] { nameof(SalePrice) });
        }

        if (SalePrice.HasValue && SalePrice.Value <= 0)
        {
            yield return new ValidationResult(
                "Sale price must be greater than zero.",
                new[] { nameof(SalePrice) });
        }

        if (SaleStartDate.HasValue &&
            SaleEndDate.HasValue &&
            SaleEndDate.Value <= SaleStartDate.Value)
        {
            yield return new ValidationResult(
                "Sale end date must be after the sale start date.",
                new[] { nameof(SaleEndDate) });
        }
    }
}