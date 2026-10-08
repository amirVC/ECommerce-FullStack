namespace ECommerceAPI.DTOs;

public class CouponValidationResultDto
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Code { get; set; }
    public string? DiscountType { get; set; }
    public decimal DiscountAmount { get; set; }
    public bool FreeShipping { get; set; }
}