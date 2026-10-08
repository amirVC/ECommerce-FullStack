namespace ECommerceAPI.Models;

public class Address
{
    public int Id { get; set; }

    // Public API identifier.
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }
    public User? User { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}