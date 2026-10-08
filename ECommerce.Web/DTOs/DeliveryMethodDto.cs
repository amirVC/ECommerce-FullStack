namespace ECommerce.Web.DTOs;

public class DeliveryMethodDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int EstimatedDaysMin { get; set; }
    public int EstimatedDaysMax { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}

public class CreateDeliveryMethodDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int EstimatedDaysMin { get; set; }
    public int EstimatedDaysMax { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class UpdateDeliveryMethodDto : CreateDeliveryMethodDto { }
