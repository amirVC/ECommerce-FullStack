
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Interfaces;

public interface ICartService
{
    Task<CartViewModel> GetCartAsync();

    Task AddToCartAsync(
        ProductViewModel product,
        int quantity = 1);

    Task IncreaseQuantityAsync(Guid publicId);

    Task DecreaseQuantityAsync(Guid publicId);

    Task RemoveFromCartAsync(Guid publicId);

    Task ClearCartAsync();

    Task<int> GetCartCountAsync();

    Task MergeGuestCartIntoUserCartAsync();

    Task<CartViewModel> ApplyCouponAsync(string code);

    Task<CartViewModel> RemoveCouponAsync();
}
