namespace ECommerceAPI.Models;

public enum CouponDiscountType
{
    Percentage,
    FixedAmount,
    FreeShipping
}

public enum CouponScope
{
    Order,
    SpecificProducts,
    SpecificCategories
}