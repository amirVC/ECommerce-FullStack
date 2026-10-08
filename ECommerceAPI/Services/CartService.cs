using ECommerceAPI.Common;
using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Extensions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    private readonly ICouponService _couponService;
    private readonly ICampaignService _campaignService;
    public CartService(
        AppDbContext context,
        ICouponService couponService,
        ICampaignService campaignService)
    {
        _context = context;
        _couponService = couponService;
        _campaignService = campaignService;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);

        return await MapToDtoAsync(cart);
    }

    public async Task<CartDto> AddItemAsync(int userId, AddToCartDto dto)
    {
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product == null)
            throw new NotFoundException($"Product {dto.ProductId} not found.");

        var cart = await GetOrCreateCartAsync(userId);

        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);
        var requestedQuantity = (existingItem?.Quantity ?? 0) + dto.Quantity;

        if (product.Stock == 0)
        {
            throw new BadRequestException($"{product.Name} is currently out of stock.");
        }

        if (product.Stock < requestedQuantity)
        {
            throw new BadRequestException($"Only {product.Stock} item(s) of {product.Name} are available.");
        }

        if (existingItem != null)
        {
            existingItem.Quantity = requestedQuantity;
        }
        else
        {
            cart.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                Product = product, 
                Quantity = dto.Quantity
            });
        }

        await RevalidateCouponAsync(userId, cart);
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(cart);
    }

    public async Task<CartDto> UpdateItemAsync(int userId, int cartItemId, UpdateCartItemDto dto)
    {
        var cartItem = await GetOwnedCartItemAsync(userId, cartItemId);

        if (cartItem.Product.Stock == 0)
        {
            throw new BadRequestException($"{cartItem.Product.Name} is currently out of stock.");
        }

        if (cartItem.Product.Stock < dto.Quantity)
        {
            throw new BadRequestException($"Only {cartItem.Product.Stock} item(s) of {cartItem.Product.Name} are available.");
        }

        cartItem.Quantity = dto.Quantity;

        var cart = await GetOrCreateCartAsync(userId);
        await RevalidateCouponAsync(userId, cart);
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(cart);
    }

    public async Task<CartDto> RemoveItemAsync(int userId, int cartItemId)
    {
        var cartItem = await GetOwnedCartItemAsync(userId, cartItemId);

        _context.CartItems.Remove(cartItem);

        var cart = await GetOrCreateCartAsync(userId);
        cart.CartItems.Remove(cart.CartItems.First(ci => ci.Id == cartItemId));
        await RevalidateCouponAsync(userId, cart);
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(cart);
    }

    public async Task<CartDto> ClearCartAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);

        _context.CartItems.RemoveRange(cart.CartItems);
        cart.CartItems.Clear(); // keep in-memory navigation in sync with the removal above
        cart.AppliedCouponCode = null;
        cart.DiscountAmount = 0m;
        cart.FreeShippingApplied = false;

        await _context.SaveChangesAsync();

        return await MapToDtoAsync(cart);
    }

    public async Task<CartDto> ApplyCouponAsync(int userId, string code)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var result = await _couponService.ValidateAsync(code, userId, cart);

        if (!result.IsValid)
            throw new BadRequestException(result.ErrorMessage ?? "Invalid coupon.");

        cart.AppliedCouponCode = result.Code;
        cart.DiscountAmount = result.DiscountAmount;
        cart.FreeShippingApplied = result.FreeShipping;
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(cart);
    }

    public async Task<CartDto> RemoveCouponAsync(int userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        cart.AppliedCouponCode = null;
        cart.DiscountAmount = 0m;
        cart.FreeShippingApplied = false;
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(cart);
    }


    private async Task<Cart> GetOrCreateCartAsync(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart != null) return cart;

        cart = new Cart { UserId = userId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        return cart;
    }

    private async Task<CartItem> GetOwnedCartItemAsync(int userId, int cartItemId)
    {
        var cartItem = await _context.CartItems
            .Include(ci => ci.Product)
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

        if (cartItem == null)
            throw new NotFoundException("Cart item not found.");

        return cartItem;
    }

    private async Task RevalidateCouponAsync(int userId, Cart cart)
    {
        if (string.IsNullOrEmpty(cart.AppliedCouponCode))
            return;

        var result = await _couponService.ValidateAsync(cart.AppliedCouponCode, userId, cart);

        if (result.IsValid)
        {
            cart.DiscountAmount = result.DiscountAmount;
            cart.FreeShippingApplied = result.FreeShipping;
        }
        else
        {
            cart.AppliedCouponCode = null;
            cart.DiscountAmount = 0m;
            cart.FreeShippingApplied = false;
        }
    }

    private async Task<CartDto> MapToDtoAsync(Cart cart)
    {
        var items = new List<CartItemDto>();

        if (cart.CartItems.Count == 0)
        {
            return new CartDto
            {
                Id = cart.Id,
                Items = items,
                TotalItems = 0,
                OriginalSubtotal = 0m,
                SaleDiscountAmount = 0m,
                CampaignDiscountAmount = 0m,
                CouponDiscountAmount = cart.DiscountAmount,
                AppliedCouponCode = cart.AppliedCouponCode,
                FreeShippingApplied = cart.FreeShippingApplied,
                DiscountAmount = cart.DiscountAmount,
                Subtotal = 0m,
                TotalAmount = 0m
            };
        }


        var campaignKeys = cart.CartItems
            .Select(ci => (
                ProductId: ci.Product.Id,
                CategoryId: ci.Product.CategoryId))
            .ToList();

        var activeCampaigns = await _campaignService
            .GetActiveCampaignsForProductsAsync(campaignKeys);

        decimal originalSubtotal = 0m;
        decimal saleDiscountTotal = 0m;
        decimal campaignDiscountTotal = 0m;

        foreach (var cartItem in cart.CartItems)
        {
            var originalUnitPrice = cartItem.Product.Price;
            var salePrice = cartItem.Product.EffectivePrice();
            var saleDiscountPerUnit = originalUnitPrice - salePrice;

            activeCampaigns.TryGetValue(cartItem.ProductId, out var campaign);
            var campaignDiscountPerUnit = CampaignService.ComputeDiscount(campaign, salePrice);

            var finalUnitPrice = Math.Max(0m, salePrice - campaignDiscountPerUnit);

            var itemSubtotal = finalUnitPrice * cartItem.Quantity;

            originalSubtotal += originalUnitPrice * cartItem.Quantity;
            saleDiscountTotal += saleDiscountPerUnit * cartItem.Quantity;
            campaignDiscountTotal += campaignDiscountPerUnit * cartItem.Quantity;

            items.Add(new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                ProductImageUrl = cartItem.Product.ImageUrl,
                ProductThumbnailUrl = cartItem.Product.ThumbnailUrl,

                OriginalUnitPrice = originalUnitPrice,
                SaleDiscountAmount = saleDiscountPerUnit * cartItem.Quantity,
                CampaignDiscountAmount = campaignDiscountPerUnit * cartItem.Quantity,
                UnitPrice = finalUnitPrice,
                IsOnSale = cartItem.Product.IsOnSale(),

                Quantity = cartItem.Quantity,
                Subtotal = itemSubtotal,
                AvailableStock = cartItem.Product.Stock
            });
        }

        var preCouponSubtotal = items.Sum(i => i.Subtotal);

        var total = Math.Max(0m, preCouponSubtotal - cart.DiscountAmount);

        return new CartDto
        {
            Id = cart.Id,
            Items = items,
            TotalItems = items.Sum(i => i.Quantity),

            OriginalSubtotal = originalSubtotal,
            SaleDiscountAmount = saleDiscountTotal,
            CampaignDiscountAmount = campaignDiscountTotal,
            CouponDiscountAmount = cart.DiscountAmount,

            AppliedCouponCode = cart.AppliedCouponCode,
            FreeShippingApplied = cart.FreeShippingApplied,

            DiscountAmount = cart.DiscountAmount,

            Subtotal = originalSubtotal,
            TotalAmount = total
        };
    }
}