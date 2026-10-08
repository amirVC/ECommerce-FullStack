using ECommerce.Web.DTOs;
using ECommerce.Web.Exceptions;
using ECommerce.Web.Helpers;
using ECommerce.Web.Interfaces;
using ECommerce.Web.Services.Http;
using ECommerce.Web.ViewModels;

namespace ECommerce.Web.Services;

public class CartService : ICartService
{
    private const string CartKey = "ShoppingCart";
    private const string CartApiBase = "api/cart";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;

    public CartService(
        IHttpContextAccessor httpContextAccessor,
        IApiClient apiClient,
        IAuthService authService)
    {
        _httpContextAccessor = httpContextAccessor;
        _apiClient = apiClient;
        _authService = authService;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    private bool IsLoggedIn =>
        !string.IsNullOrWhiteSpace(_authService.GetToken());

    public async Task<CartViewModel> GetCartAsync()
    {
        if (IsLoggedIn)
        {
            var cart = await _apiClient.GetAsync<CartDto>(CartApiBase);
            return MapFromApi(cart);
        }

        return MapFromSession(GetSessionCart());
    }

    public async Task AddToCartAsync(
        ProductViewModel product,
        int quantity = 1)
    {
        if (IsLoggedIn)
        {
            await _apiClient.PostAsync<AddToCartDto, CartDto>(
                $"{CartApiBase}/items",
                new AddToCartDto
                {
                    ProductId = product.Id,
                    Quantity = quantity
                });

            return;
        }

        var cart = GetSessionCart();

        var item = cart.FirstOrDefault(
            x => x.ProductId == product.Id);

        if (item == null)
        {
            cart.Add(new CartItemViewModel
            {
                Id = product.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.EffectivePrice,
                OriginalPrice = product.Price,
                IsOnSale = product.IsOnSale,
                Quantity = quantity,
                ImageUrl = product.ImageUrl,    
                ThumbnailUrl = product.ThumbnailUrl   // NEW

            });
        }
        else
        {
            item.Quantity += quantity;
        }

        SaveSessionCart(cart);
    }

    public async Task IncreaseQuantityAsync(int id)
    {
        if (IsLoggedIn)
        {
            var cart =
                await _apiClient.GetAsync<CartDto>(CartApiBase);

            var item =
                cart?.Items.FirstOrDefault(i => i.Id == id);

            if (item == null)
                return;

            await _apiClient.PutAsync<UpdateCartItemDto, CartDto>(
                $"{CartApiBase}/items/{id}",
                new UpdateCartItemDto
                {
                    Quantity = item.Quantity + 1
                });

            return;
        }

        var sessionCart = GetSessionCart();

        var sessionItem =
            sessionCart.FirstOrDefault(x => x.ProductId == id);

        if (sessionItem == null)
            return;

        sessionItem.Quantity++;

        SaveSessionCart(sessionCart);
    }

    public async Task DecreaseQuantityAsync(int id)
    {
        if (IsLoggedIn)
        {
            var cart =
                await _apiClient.GetAsync<CartDto>(CartApiBase);

            var item =
                cart?.Items.FirstOrDefault(i => i.Id == id);

            if (item == null)
                return;

            if (item.Quantity <= 1)
            {
                await _apiClient.DeleteAsync<CartDto>(
                    $"{CartApiBase}/items/{id}");
            }
            else
            {
                await _apiClient.PutAsync<UpdateCartItemDto, CartDto>(
                    $"{CartApiBase}/items/{id}",
                    new UpdateCartItemDto
                    {
                        Quantity = item.Quantity - 1
                    });
            }

            return;
        }

        var sessionCart = GetSessionCart();

        var sessionItem =
            sessionCart.FirstOrDefault(x => x.ProductId == id);

        if (sessionItem == null)
            return;

        sessionItem.Quantity--;

        if (sessionItem.Quantity <= 0)
            sessionCart.Remove(sessionItem);

        SaveSessionCart(sessionCart);
    }

    public async Task RemoveFromCartAsync(int id)
    {
        if (IsLoggedIn)
        {
            await _apiClient.DeleteAsync<CartDto>(
                $"{CartApiBase}/items/{id}");

            return;
        }

        var cart = GetSessionCart();

        var item =
            cart.FirstOrDefault(x => x.ProductId == id);

        if (item == null)
            return;

        cart.Remove(item);

        SaveSessionCart(cart);
    }

    public async Task ClearCartAsync()
    {
        if (IsLoggedIn)
        {
            await _apiClient.DeleteAsync<CartDto>(CartApiBase);
            return;
        }

        SaveSessionCart(
            new List<CartItemViewModel>());
    }

    public async Task<int> GetCartCountAsync()
    {
        var cart = await GetCartAsync();

        return cart.Items.Sum(x => x.Quantity);
    }

    public async Task<CartViewModel> ApplyCouponAsync(string code)
    {
        if (!IsLoggedIn)
        {
            throw new ApiException(
                "Please log in to use a coupon code.",
                401);
        }

        var cart =
            await _apiClient.PostAsync<ApplyCouponDto, CartDto>(
                $"{CartApiBase}/coupon",
                new ApplyCouponDto
                {
                    Code = code
                });

        return MapFromApi(cart);
    }

    public async Task<CartViewModel> RemoveCouponAsync()
    {
        if (!IsLoggedIn)
        {
            throw new ApiException(
                "Please log in to use a coupon code.",
                401);
        }

        var cart =
            await _apiClient.DeleteAsync<CartDto>(
                $"{CartApiBase}/coupon");

        return MapFromApi(cart);
    }

    // ---------- Helpers ----------

    private List<CartItemViewModel> GetSessionCart()
        => Session.GetObject<List<CartItemViewModel>>(CartKey)
           ?? new List<CartItemViewModel>();

    private void SaveSessionCart(
        List<CartItemViewModel> cart)
        => Session.SetObject(CartKey, cart);

    private static CartViewModel MapFromApi(
        CartDto? cart)
    {
        if (cart == null)
        {
            return new CartViewModel
            {
                CanApplyCoupon = true
            };
        }

        var items = cart.Items
            .Select(i => new CartItemViewModel
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.UnitPrice,
                OriginalPrice = i.OriginalUnitPrice,
                IsOnSale = i.IsOnSale,
                Quantity = i.Quantity,
                ImageUrl = i.ProductImageUrl,
                ThumbnailUrl = i.ProductThumbnailUrl,   
                AvailableStock = i.AvailableStock
            })
            .ToList();            

        return new CartViewModel
        {
            Items = items,

            // Original/main price.
            Subtotal = cart.Subtotal,

            // Sale discount.
            SaleDiscountAmount =
                cart.SaleDiscountAmount,

            // Campaign discount.
            CampaignDiscountAmount =
                cart.CampaignDiscountAmount,

            // Coupon information.
            AppliedCouponCode =
                cart.AppliedCouponCode,

            DiscountAmount =
                cart.DiscountAmount,

            FreeShippingApplied =
                cart.FreeShippingApplied,

            // Final amount after:
            // Sale + Campaign + Coupon.
            Total =
                cart.TotalAmount,

            CanApplyCoupon = true
        };
    }

    private static CartViewModel MapFromSession(
        List<CartItemViewModel> items)
    {
        var subtotal =
            items.Sum(i => i.Total);

        return new CartViewModel
        {
            Items = items,

            Subtotal = subtotal,

            SaleDiscountAmount = 0m,

            CampaignDiscountAmount = 0m,

            AppliedCouponCode = null,

            DiscountAmount = 0m,

            FreeShippingApplied = false,

            Total = subtotal,

            CanApplyCoupon = false
        };
    }

    public async Task MergeGuestCartIntoUserCartAsync()
    {
        if (!IsLoggedIn)
            return;

        var guestCart = GetSessionCart();

        if (!guestCart.Any())
            return;

        var allMergedSuccessfully = true;

        foreach (var item in guestCart)
        {
            try
            {
                await _apiClient.PostAsync<AddToCartDto, CartDto>(
                    $"{CartApiBase}/items",
                    new AddToCartDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    });
            }
            catch
            {
                allMergedSuccessfully = false;
            }
        }

        if (allMergedSuccessfully)
        {
            SaveSessionCart(
                new List<CartItemViewModel>());
        }
    }
}