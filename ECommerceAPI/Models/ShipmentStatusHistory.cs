namespace ECommerceAPI.Models;

public class ShipmentStatusHistory
{
    public int Id { get; set; }
    public int ShipmentId { get; set; }
    public Shipment? Shipment { get; set; }

    public ShipmentStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}