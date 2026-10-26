using System.Security.Claims;
using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly AppDbContext _context;
        private readonly IShipmentService _shipmentService;

        public OrdersController(
            IOrderService orderService,
            AppDbContext context,
            IShipmentService shipmentService)
        {
            _orderService = orderService;
            _context = context;
            _shipmentService = shipmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier)!);

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var result = orders.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                Status = o.Status,
                TotalAmount = o.TotalAmount,
                OrderDate = o.OrderDate,
                PublicId = o.PublicId,
                FullName = o.FullName,
                PhoneNumber = o.PhoneNumber,
                Address = o.Address,
                City = o.City,
                PostalCode = o.PostalCode,
                Country = o.Country,
                Notes = o.Notes,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,

                CouponCode = o.CouponCode,
                DiscountAmount = o.DiscountAmount,
                CampaignDiscountAmount =
                    o.CampaignDiscountAmount,
                ShippingCost = o.ShippingCost,

                Items = o.OrderItems.Select(oi =>
                    new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Subtotal =
                            oi.Quantity * oi.UnitPrice
                    }).ToList()
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier)!);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(
                    o => o.Id == id &&
                         o.UserId == userId);

            if (order == null)
                return NotFound("Order not found.");

            var result = new OrderResponseDto
            {
                Id = order.Id,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                OrderDate = order.OrderDate,
                PublicId = order.PublicId,
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
                CampaignDiscountAmount =
                    order.CampaignDiscountAmount,
                ShippingCost = order.ShippingCost,

                Items = order.OrderItems.Select(oi =>
                    new OrderItemResponseDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Subtotal =
                            oi.Quantity * oi.UnitPrice
                    }).ToList()
            };

            return Ok(result);
        }

        [HttpGet("public/{publicId:guid}")]
        public async Task<IActionResult> GetByPublicId(Guid publicId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.PublicId == publicId && o.UserId == userId);

            if (order == null)
                return NotFound("Order not found.");

            var result = new OrderResponseDto
            {
                Id = order.Id,
                PublicId = order.PublicId,
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
                    Subtotal = oi.Quantity * oi.UnitPrice
                }).ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        [EnableRateLimiting("order")]
        public async Task<IActionResult> CreateOrder(
            CheckoutDto dto)
        {
            var userId =
                int.Parse(
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier)!);

            var result =
                await _orderService.CheckoutAsync(
                    dto,
                    userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] string status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
                return NotFound("Order not found.");

            var validStatuses = new[]
            {
                "Pending",
                "Shipped",
                "Delivered",
                "Cancelled"
            };

            if (!validStatuses.Contains(status))
                return BadRequest("Invalid status.");

            order.Status = status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Status updated.",
                orderId = id,
                status
            });
        }

        [HttpGet("{orderId}/shipment")]
        [Authorize]
        public async Task<IActionResult> GetShipment(
            int orderId)
        {
            var userId =
                int.Parse(
                    User.FindFirst(
                        ClaimTypes.NameIdentifier)!.Value);

            var isAdmin = User.IsInRole("Admin");

            var shipment =
                await _shipmentService.GetByOrderIdAsync(
                    orderId,
                    userId,
                    isAdmin);

            return shipment == null
                ? NotFound()
                : Ok(shipment);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var result = orders.Select(o =>
                new OrderResponseDto
                {
                    Id = o.Id,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate,
                    PublicId = o.PublicId,
                    FullName = o.FullName,
                    PhoneNumber = o.PhoneNumber,
                    Address = o.Address,
                    City = o.City,
                    PostalCode = o.PostalCode,
                    Country = o.Country,
                    Notes = o.Notes,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,

                    CouponCode = o.CouponCode,
                    DiscountAmount = o.DiscountAmount,
                    CampaignDiscountAmount =
                        o.CampaignDiscountAmount,
                    ShippingCost = o.ShippingCost,

                    Items = o.OrderItems.Select(oi =>
                        new OrderItemResponseDto
                        {
                            ProductId = oi.ProductId,
                            ProductName =
                                oi.Product.Name,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            Subtotal =
                                oi.Quantity *
                                oi.UnitPrice
                        }).ToList()
                });

            return Ok(result);
        }
    }
}
