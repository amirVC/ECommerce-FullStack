namespace ECommerceAPI.Models;

public class DeliveryMethod
{
    public int Id { get; set; }

    // Public API identifier.
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;          // "Standard", "Express"
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int EstimatedDaysMin { get; set; }
    public int EstimatedDaysMax { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}