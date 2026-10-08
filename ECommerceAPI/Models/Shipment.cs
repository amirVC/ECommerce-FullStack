namespace ECommerceAPI.Models;

public class Shipment
{
    public int Id { get; set; }

    // Public API identifier.
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Processing;

    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ShipmentStatusHistory> StatusHistory { get; set; } = new List<ShipmentStatusHistory>();
}