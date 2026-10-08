namespace ECommerce.Web.DTOs;

public class ShipmentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public List<ShipmentStatusHistoryDto> History { get; set; } = new();
}

public class ShipmentStatusHistoryDto
{
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public DateTime ChangedAt { get; set; }
}

public class CreateShipmentDto
{
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
}

public class UpdateShipmentStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
}
