using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Extensions;
using ECommerceAPI.Models;
using ECommerceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly ICouponService _couponService;
    private readonly ICampaignService _campaignService;
    public OrderService(
        AppDbContext context,
        ICouponService couponService,
        ICampaignService campaignService)
    {
        _context = context;
        _couponService = couponService;
        _campaignService = campaignService;
    }
    public async Task<OrderResponseDto> CheckoutAsync(
        CheckoutDto dto,
        int userId)
    {
        if (dto.Items == null || dto.Items.Count == 0)
            throw new BadRequestException("Order must have at least one item.");

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                UserId = userId,

                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                City = dto.City,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                Notes = dto.Notes,
                PaymentMethod = dto.PaymentMethod,

                PaymentStatus = "Pending"
            };


            var requestedProductIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();

            var productsById = await _context.Products
                .Where(p => requestedProductIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var item in dto.Items)
            {
                if (!productsById.ContainsKey(item.ProductId))
                    throw new NotFoundException($"Product {item.ProductId} not found.");
            }


            var campaignKeys = productsById.Values
                .Select(p => (ProductId: p.Id, CategoryId: p.CategoryId))
                .ToList();

            var activeCampaigns = await _campaignService
                .GetActiveCampaignsForProductsAsync(campaignKeys);

            decimal subtotal = 0;
            decimal campaignDiscountAmount = 0;
            var validationCart = new Cart { CartItems = new List<CartItem>() };

            foreach (var item in dto.Items)
            {
                var product = productsById[item.ProductId];

                if (product.Stock < item.Quantity)
                    throw new BadRequestException($"Not enough stock for {product.Name}.");

                product.Stock -= item.Quantity;

                var basePrice = product.EffectivePrice();

                activeCampaigns.TryGetValue(product.Id, out var campaign);
                var campaignDiscount = CampaignService.ComputeDiscount(campaign, basePrice);

                var finalUnitPrice = Math.Max(0m, basePrice - campaignDiscount);

                campaignDiscountAmount += campaignDiscount * item.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Product = product, // wire up directly — avoids re-querying the saved order at the end
                    SKU = product.SKU, // snapshot — stays correct even if the product's SKU changes later
                    Quantity = item.Quantity,
                    UnitPrice = finalUnitPrice
                });

                subtotal += finalUnitPrice * item.Quantity;
                validationCart.CartItems.Add(new CartItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = item.Quantity
                });
            }

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            decimal discountAmount = 0;
            string? appliedCode = null;

            if (cart != null && !string.IsNullOrEmpty(cart.AppliedCouponCode))
            {
                var result = await _couponService.ValidateAsync(
                    cart.AppliedCouponCode, userId, validationCart);

                if (!result.IsValid)
                {
                    throw new BadRequestException(
                        $"Your coupon is no longer valid: {result.ErrorMessage}");
                }

                discountAmount = result.DiscountAmount;
                appliedCode = result.Code;
            }

            order.CouponCode = appliedCode;
            order.DiscountAmount = discountAmount;
            order.CampaignDiscountAmount = campaignDiscountAmount;

            var freeShipping = cart != null &&
                               cart.FreeShippingApplied &&
                               appliedCode != null;

            order.ShippingCost = freeShipping ? 0m : dto.ShippingCost;

            order.TotalAmount = Math.Max(0, subtotal - discountAmount) + order.ShippingCost;

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            if (appliedCode != null)
            {
                await _couponService.RecordUsageAsync(appliedCode, userId, order.Id, discountAmount);
            }

            if (cart != null)
            {
                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();
                _context.CartItems.RemoveRange(cartItems);

                cart.AppliedCouponCode = null;
                cart.DiscountAmount = 0m;
                cart.FreeShippingApplied = false;

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return new OrderResponseDto
            {
                Id = order.Id,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,

                FullName = order.FullName,
                PhoneNumber = order.PhoneNumber,
                Address = order.Address,
                City = order.City,
                PostalCode = order.PostalCode,
                Country = order.Country,
                Notes = order.Notes,

                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                CouponCode = order.CouponCode,
                DiscountAmount = order.DiscountAmount,
                CampaignDiscountAmount = order.CampaignDiscountAmount,
                ShippingCost = order.ShippingCost,

                Items = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    SKU = oi.SKU,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.UnitPrice * oi.Quantity
                }).ToList()
            };
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}