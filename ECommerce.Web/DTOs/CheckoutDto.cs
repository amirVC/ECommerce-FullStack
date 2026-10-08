namespace ECommerce.Web.DTOs;

public class CheckoutDto
{
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string PaymentMethod { get; set; } = "Cash";

    public decimal ShippingCost { get; set; } = 0m;

    public List<OrderItemDto> Items { get; set; } = new();
    public int AddressId { get; set; }
    public int DeliveryMethodId { get; set; }
}