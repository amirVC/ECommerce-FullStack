using ECommerce.Web.DTOs;

namespace ECommerce.Web.ViewModels;

public class CheckoutViewModel
{
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string PaymentMethod { get; set; } = "Cash";

    // Phase 18: shipping
    public int AddressId { get; set; }
    public int DeliveryMethodId { get; set; }

    public List<AddressDto> AvailableAddresses { get; set; } = new();
    public List<DeliveryMethodDto> AvailableDeliveryMethods { get; set; } = new();
}
