namespace ECommerceAPI.DTOs
{
    // AddressDto.cs
    public class AddressDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    // CreateAddressDto.cs / UpdateAddressDto.cs — same shape minus Id
    public class CreateAddressDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
    public class UpdateAddressDto : CreateAddressDto { }

    // DeliveryMethodDto.cs
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

}
