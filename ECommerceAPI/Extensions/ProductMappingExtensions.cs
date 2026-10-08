namespace ECommerceAPI.Extensions;

using ECommerceAPI.DTOs;
using ECommerceAPI.Models;

public static class ProductMappingExtensions
{
    public static ProductResponseDto ToResponseDto(this Product product)
    {
        var effectivePrice = product.EffectivePrice();
        var finalPrice = effectivePrice;

        return new ProductResponseDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            ThumbnailUrl = product.ThumbnailUrl,  

            CategoryName = product.Category?.Name ?? string.Empty,

            Images = product.Images?.Select(img => new ProductImageDto
            {
                Id = img.Id,
                ImageUrl = img.ImageUrl,
                ThumbnailUrl = img.ThumbnailUrl,   // NEW
                IsPrimary = img.IsPrimary,
                AltText = img.AltText,
                DisplayOrder = img.DisplayOrder
            }).ToList() ?? new List<ProductImageDto>(),

            SalePrice = product.SalePrice,
            SaleStartDate = product.SaleStartDate,
            SaleEndDate = product.SaleEndDate,
            IsOnSale = product.IsOnSale(),
            EffectivePrice = effectivePrice,

            CampaignDiscount = 0,
            HasCampaign = false,
            CampaignName = null,

            FinalPrice = finalPrice,
            TotalDiscountAmount = product.Price - finalPrice
        };
    }
}