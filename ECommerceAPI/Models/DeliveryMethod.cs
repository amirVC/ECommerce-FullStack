namespace ECommerceAPI.Models;

public class DeliveryMethod
{
    public int Id { get; set; }

    public Guid PublicId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;         
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int EstimatedDaysMin { get; set; }
    public int EstimatedDaysMax { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}