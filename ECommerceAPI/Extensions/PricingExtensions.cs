using ECommerceAPI.Models;

namespace ECommerceAPI.Extensions;

public static class PricingExtensions
{
    public static bool IsOnSale(this Product product)
    {
        var now = DateTime.UtcNow;
        return product.SalePrice.HasValue
            && product.SalePrice.Value < product.Price
            && (!product.SaleStartDate.HasValue || product.SaleStartDate <= now)
            && (!product.SaleEndDate.HasValue || product.SaleEndDate >= now);
    }

    public static decimal EffectivePrice(this Product product)
        => product.IsOnSale() ? product.SalePrice!.Value : product.Price;
}