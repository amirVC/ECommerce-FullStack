namespace ECommerceAPI.Models;

public enum ShipmentStatus
{
    Processing = 0,
    Shipped = 1,
    InTransit = 2,
    Delivered = 3,
    FailedAttempt = 4,
    Returned = 5
}