using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Models;
using ECommerceAPI.Services;
using ECommerceAPI.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ECommerceAPI.UnitTests.Services;

public class OrderServiceTests
{
    // ---- test helpers -------------------------------------------------------

    private static AppDbContext CreateContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static Product MakeProduct(
        int id,
        decimal price,
        int stock,
        string name = "Product",
        decimal? salePrice = null,
        DateTime? saleStart = null,
        DateTime? saleEnd = null,
        int categoryId = 1)
    {
        return new Product
        {
            Id = id,
            Name = name,
            Description = "desc",
            Price = price,
            Stock = stock,
            CategoryId = categoryId,
            SalePrice = salePrice,
            SaleStartDate = saleStart,
            SaleEndDate = saleEnd
        };
    }

    private static CheckoutDto MakeCheckoutDto(
        List<OrderItemDto>? items = null,
        decimal shippingCost = 0m,
        string paymentMethod = "Cash")
    {
        return new CheckoutDto
        {
            FullName = "Jane Doe",
            PhoneNumber = "555-1234",
            Address = "1 Main St",
            City = "Baku",
            PostalCode = "AZ1000",
            Country = "Azerbaijan",
            PaymentMethod = paymentMethod,
            ShippingCost = shippingCost,
            Items = items ?? new List<OrderItemDto>()
        };
    }

    private static (Mock<ICouponService> Coupon, Mock<ICampaignService> Campaign) CreateDefaultMocks()
    {
        var couponMock = new Mock<ICouponService>();

        var campaignMock = new Mock<ICampaignService>();
        // Default: no active campaigns for any product.
        campaignMock
            .Setup(c => c.GetActiveCampaignsForProductsAsync(
                It.IsAny<IEnumerable<(int ProductId, int CategoryId)>>()))
            .ReturnsAsync(new Dictionary<int, CampaignDto>());

        return (couponMock, campaignMock);
    }

    private static OrderService CreateService(
        AppDbContext context,
        Mock<ICouponService> couponMock,
        Mock<ICampaignService> campaignMock)
    {
        return new OrderService(context, couponMock.Object, campaignMock.Object);
    }

    // ---- input validation -----------------------------------------------------

    [Fact]
    public async Task CheckoutAsync_ThrowsBadRequestException_WhenItemsIsNull()
    {
        using var context = CreateContext();
        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto();
        dto.Items = null!;

        var act = async () => await service.CheckoutAsync(dto, userId: 1);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Order must have at least one item.");
    }

    [Fact]
    public async Task CheckoutAsync_ThrowsBadRequestException_WhenItemsIsEmpty()
    {
        using var context = CreateContext();
        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>());

        var act = async () => await service.CheckoutAsync(dto, userId: 1);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Order must have at least one item.");
    }

    [Fact]
    public async Task CheckoutAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
    {
        using var context = CreateContext();
        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 999, Quantity = 1 }
        });

        var act = async () => await service.CheckoutAsync(dto, userId: 1);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product 999 not found.");
    }

    [Fact]
    public async Task CheckoutAsync_ThrowsBadRequestException_WhenInsufficientStock()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(id: 1, price: 10m, stock: 2, name: "Widget"));
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 1, Quantity = 5 }
        });

        var act = async () => await service.CheckoutAsync(dto, userId: 1);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Not enough stock for Widget.");
    }

    // ---- happy path -------------------------------------------------------------

    [Fact]
    public async Task CheckoutAsync_CreatesOrder_WithCorrectSubtotalAndTotal_WhenNoDiscounts()
    {
        using var context = CreateContext();
        context.Products.AddRange(
            MakeProduct(id: 1, price: 10m, stock: 5, name: "Widget"),
            MakeProduct(id: 2, price: 25m, stock: 5, name: "Gadget"));
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(
            items: new List<OrderItemDto>
            {
                new() { ProductId = 1, Quantity = 2 }, // 20
                new() { ProductId = 2, Quantity = 1 }  // 25
            },
            shippingCost: 4.99m);

        var result = await service.CheckoutAsync(dto, userId: 7);

        // subtotal = 45, discount = 0 => total = 45 + 4.99
        result.TotalAmount.Should().Be(49.99m);
        result.DiscountAmount.Should().Be(0m);
        result.CampaignDiscountAmount.Should().Be(0m);
        result.ShippingCost.Should().Be(4.99m);
        result.Status.Should().Be("Pending");
        result.PaymentStatus.Should().Be("Pending");

        result.Items.Should().HaveCount(2);
        result.Items.Should().ContainSingle(i => i.ProductId == 1 && i.UnitPrice == 10m && i.Subtotal == 20m);
        result.Items.Should().ContainSingle(i => i.ProductId == 2 && i.UnitPrice == 25m && i.Subtotal == 25m);
    }

    [Fact]
    public async Task CheckoutAsync_PersistsOrder_AndDecrementsProductStock()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        context.Products.Add(MakeProduct(id: 1, price: 10m, stock: 5, name: "Widget"));
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 1, Quantity = 3 }
        });

        var result = await service.CheckoutAsync(dto, userId: 7);

        using var verifyContext = CreateContext(dbName);

        var savedOrder = await verifyContext.Orders
            .Include(o => o.OrderItems)
            .SingleAsync(o => o.Id == result.Id);

        savedOrder.UserId.Should().Be(7);
        savedOrder.OrderItems.Should().ContainSingle(i => i.ProductId == 1 && i.Quantity == 3);

        var savedProduct = await verifyContext.Products.SingleAsync(p => p.Id == 1);
        savedProduct.Stock.Should().Be(2); // 5 - 3
    }

    [Fact]
    public async Task CheckoutAsync_UsesSalePrice_WhenProductIsCurrentlyOnSale()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(
            id: 1,
            price: 100m,
            stock: 5,
            name: "Widget",
            salePrice: 60m,
            saleStart: DateTime.UtcNow.AddDays(-1),
            saleEnd: DateTime.UtcNow.AddDays(1)));
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 1, Quantity = 1 }
        });

        var result = await service.CheckoutAsync(dto, userId: 1);

        result.Items.Single().UnitPrice.Should().Be(60m);
        result.TotalAmount.Should().Be(60m);
    }

    [Fact]
    public async Task CheckoutAsync_IgnoresSalePrice_WhenSaleWindowHasNotStartedYet()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(
            id: 1,
            price: 100m,
            stock: 5,
            name: "Widget",
            salePrice: 60m,
            saleStart: DateTime.UtcNow.AddDays(1),
            saleEnd: DateTime.UtcNow.AddDays(5)));
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 1, Quantity = 1 }
        });

        var result = await service.CheckoutAsync(dto, userId: 1);

        result.Items.Single().UnitPrice.Should().Be(100m);
    }

    // ---- campaign discounts -------------------------------------------------------

    [Fact]
    public async Task CheckoutAsync_AppliesPercentageCampaignDiscount_ToUnitPrice()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(id: 1, price: 100m, stock: 5, name: "Widget", categoryId: 3));
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        campaign
            .Setup(c => c.GetActiveCampaignsForProductsAsync(
                It.IsAny<IEnumerable<(int ProductId, int CategoryId)>>()))
            .ReturnsAsync(new Dictionary<int, CampaignDto>
            {
                [1] = new CampaignDto
                {
                    Id = 1,
                    Name = "20% Off",
                    DiscountType = (int)CampaignDiscountType.Percentage,
                    DiscountValue = 20m,
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    IsActive = true
                }
            });

        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 1, Quantity = 2 }
        });

        var result = await service.CheckoutAsync(dto, userId: 1);

        // basePrice 100, 20% off => 80 per unit, x2 = 160
        result.Items.Single().UnitPrice.Should().Be(80m);
        result.CampaignDiscountAmount.Should().Be(40m); // 20 discount * 2 units
        result.TotalAmount.Should().Be(160m);
    }

    [Fact]
    public async Task CheckoutAsync_AppliesFixedAmountCampaignDiscount_ButNeverGoesNegative()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(id: 1, price: 5m, stock: 5, name: "Widget"));
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        campaign
            .Setup(c => c.GetActiveCampaignsForProductsAsync(
                It.IsAny<IEnumerable<(int ProductId, int CategoryId)>>()))
            .ReturnsAsync(new Dictionary<int, CampaignDto>
            {
                [1] = new CampaignDto
                {
                    Id = 1,
                    Name = "10 Off",
                    DiscountType = (int)CampaignDiscountType.FixedAmount,
                    DiscountValue = 10m, // larger than the 5m base price
                    StartDate = DateTime.UtcNow.AddDays(-1),
                    EndDate = DateTime.UtcNow.AddDays(1),
                    IsActive = true
                }
            });

        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 1, Quantity = 1 }
        });

        var result = await service.CheckoutAsync(dto, userId: 1);

        result.Items.Single().UnitPrice.Should().Be(0m); // clamped at 0, never negative
        result.TotalAmount.Should().Be(0m);
    }

    // ---- coupon discounts (via cart) -------------------------------------------------

    [Fact]
    public async Task CheckoutAsync_AppliesCouponDiscount_AndRecordsUsage_WhenCartHasValidCoupon()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(id: 1, price: 50m, stock: 5, name: "Widget"));
        context.Carts.Add(new Cart
        {
            UserId = 1,
            AppliedCouponCode = "SAVE10",
            CartItems = new List<CartItem>
            {
                new() { ProductId = 1, Quantity = 1 }
            }
        });
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        coupon
            .Setup(c => c.ValidateAsync("SAVE10", 1, It.IsAny<Cart>()))
            .ReturnsAsync(new CouponValidationResultDto
            {
                IsValid = true,
                Code = "SAVE10",
                DiscountAmount = 10m
            });

        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 1, Quantity = 1 }
        });

        var result = await service.CheckoutAsync(dto, userId: 1);

        result.CouponCode.Should().Be("SAVE10");
        result.DiscountAmount.Should().Be(10m);
        result.TotalAmount.Should().Be(40m); // 50 - 10

        coupon.Verify(c => c.RecordUsageAsync("SAVE10", 1, result.Id, 10m), Times.Once);
    }

    [Fact]
    public async Task CheckoutAsync_ThrowsBadRequestException_WhenCartCouponIsNoLongerValid()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        context.Products.Add(MakeProduct(id: 1, price: 50m, stock: 5, name: "Widget"));
        context.Carts.Add(new Cart { UserId = 1, AppliedCouponCode = "EXPIRED" });
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        coupon
            .Setup(c => c.ValidateAsync("EXPIRED", 1, It.IsAny<Cart>()))
            .ReturnsAsync(new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "Coupon expired"
            });

        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 1, Quantity = 1 }
        });

        var act = async () => await service.CheckoutAsync(dto, userId: 1);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Your coupon is no longer valid: Coupon expired");

        coupon.Verify(c => c.RecordUsageAsync(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);

        // Nothing should have been persisted: no order created and stock untouched.
        using var verifyContext = CreateContext(dbName);
        (await verifyContext.Orders.CountAsync()).Should().Be(0);
        (await verifyContext.Products.SingleAsync(p => p.Id == 1)).Stock.Should().Be(5);
    }

    [Fact]
    public async Task CheckoutAsync_ClampsTotalToZero_WhenCouponDiscountExceedsSubtotal()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(id: 1, price: 10m, stock: 5, name: "Widget"));
        context.Carts.Add(new Cart { UserId = 1, AppliedCouponCode = "HUGE" });
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        coupon
            .Setup(c => c.ValidateAsync("HUGE", 1, It.IsAny<Cart>()))
            .ReturnsAsync(new CouponValidationResultDto
            {
                IsValid = true,
                Code = "HUGE",
                DiscountAmount = 500m // way more than the 10m subtotal
            });

        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(
            items: new List<OrderItemDto> { new() { ProductId = 1, Quantity = 1 } },
            shippingCost: 5m);

        var result = await service.CheckoutAsync(dto, userId: 1);

        // Math.Max(0, subtotal - discount) + shipping => 0 + 5
        result.TotalAmount.Should().Be(5m);
    }

    [Fact]
    public async Task CheckoutAsync_AppliesFreeShipping_WhenCartHasFreeShippingAndValidCouponApplied()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(id: 1, price: 50m, stock: 5, name: "Widget"));
        context.Carts.Add(new Cart
        {
            UserId = 1,
            AppliedCouponCode = "FREESHIP",
            FreeShippingApplied = true
        });
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        coupon
            .Setup(c => c.ValidateAsync("FREESHIP", 1, It.IsAny<Cart>()))
            .ReturnsAsync(new CouponValidationResultDto
            {
                IsValid = true,
                Code = "FREESHIP",
                DiscountAmount = 0m,
                FreeShipping = true
            });

        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(
            items: new List<OrderItemDto> { new() { ProductId = 1, Quantity = 1 } },
            shippingCost: 14.99m);

        var result = await service.CheckoutAsync(dto, userId: 1);

        result.ShippingCost.Should().Be(0m);
    }

    [Fact]
    public async Task CheckoutAsync_DoesNotApplyFreeShipping_WhenCartHasNoAppliedCoupon()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(id: 1, price: 50m, stock: 5, name: "Widget"));
        context.Carts.Add(new Cart
        {
            UserId = 1,
            AppliedCouponCode = null,
            FreeShippingApplied = true // stale/irrelevant flag with no active coupon
        });
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(
            items: new List<OrderItemDto> { new() { ProductId = 1, Quantity = 1 } },
            shippingCost: 14.99m);

        var result = await service.CheckoutAsync(dto, userId: 1);

        result.ShippingCost.Should().Be(14.99m);
        coupon.Verify(c => c.ValidateAsync(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_RemovesCartItems_AndResetsCartCouponState_AfterSuccessfulCheckout()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(id: 1, price: 50m, stock: 5, name: "Widget"));
        var cart = new Cart
        {
            UserId = 1,
            AppliedCouponCode = "SAVE10",
            DiscountAmount = 10m,
            FreeShippingApplied = true,
            CartItems = new List<CartItem> { new() { ProductId = 1, Quantity = 1 } }
        };
        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        coupon
            .Setup(c => c.ValidateAsync("SAVE10", 1, It.IsAny<Cart>()))
            .ReturnsAsync(new CouponValidationResultDto { IsValid = true, Code = "SAVE10", DiscountAmount = 10m });

        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 1, Quantity = 1 }
        });

        await service.CheckoutAsync(dto, userId: 1);

        (await context.CartItems.CountAsync(ci => ci.CartId == cart.Id)).Should().Be(0);

        var reloadedCart = await context.Carts.SingleAsync(c => c.Id == cart.Id);
        reloadedCart.AppliedCouponCode.Should().BeNull();
        reloadedCart.DiscountAmount.Should().Be(0m);
        reloadedCart.FreeShippingApplied.Should().BeFalse();
    }

    [Fact]
    public async Task CheckoutAsync_Succeeds_WhenUserHasNoCart()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(id: 1, price: 50m, stock: 5, name: "Widget"));
        await context.SaveChangesAsync();
        // Note: no Cart row for this user at all.

        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(
            items: new List<OrderItemDto> { new() { ProductId = 1, Quantity = 1 } },
            shippingCost: 4.99m);

        var result = await service.CheckoutAsync(dto, userId: 1);

        result.TotalAmount.Should().Be(54.99m);
        result.DiscountAmount.Should().Be(0m);
        coupon.Verify(c => c.ValidateAsync(
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Cart>()), Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_DeductsStockForEachOccurrence_WhenSameProductAppearsInMultipleLineItems()
    {
        using var context = CreateContext();
        context.Products.Add(MakeProduct(id: 1, price: 10m, stock: 10, name: "Widget"));
        await context.SaveChangesAsync();

        var (coupon, campaign) = CreateDefaultMocks();
        var service = CreateService(context, coupon, campaign);

        var dto = MakeCheckoutDto(items: new List<OrderItemDto>
        {
            new() { ProductId = 1, Quantity = 2 },
            new() { ProductId = 1, Quantity = 3 }
        });

        var result = await service.CheckoutAsync(dto, userId: 1);

        result.Items.Should().HaveCount(2);
        var product = await context.Products.SingleAsync(p => p.Id == 1);
        product.Stock.Should().Be(5); // 10 - 2 - 3
    }
}