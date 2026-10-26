using System.Security.Claims;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int UserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetCartAsync(UserId);
            return Ok(cart);
        }

        [HttpPost("items")]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> AddItem(
            [FromBody] AddToCartDto dto)
        {
            var cart = await _cartService.AddItemAsync(UserId, dto);
            return Ok(cart);
        }

        [HttpPut("items/{publicId:guid}")]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> UpdateItem(
            Guid publicId,
            [FromBody] UpdateCartItemDto dto)
        {
            var cart = await _cartService.UpdateItemAsync(
                UserId,
                publicId,
                dto);

            return Ok(cart);
        }

        [HttpDelete("items/{publicId:guid}")]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> RemoveItem(Guid publicId)
        {
            var cart = await _cartService.RemoveItemAsync(
                UserId,
                publicId);

            return Ok(cart);
        }

        [HttpPost("items/{publicId:guid}/increase")]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> IncreaseQuantity(Guid publicId)
        {
            await _cartService.IncreaseQuantityAsync(publicId, UserId);

            return Ok(await _cartService.GetCartAsync(UserId));
        }

        [HttpPost("items/{publicId:guid}/decrease")]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> DecreaseQuantity(Guid publicId)
        {
            await _cartService.DecreaseQuantityAsync(publicId, UserId);

            return Ok(await _cartService.GetCartAsync(UserId));
        }

        [HttpDelete]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> ClearCart()
        {
            var cart = await _cartService.ClearCartAsync(UserId);
            return Ok(cart);
        }

        [HttpPost("coupon")]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> ApplyCoupon(
            [FromBody] ApplyCouponDto dto)
        {
            var cart = await _cartService.ApplyCouponAsync(
                UserId,
                dto.Code);

            return Ok(cart);
        }

        [HttpDelete("coupon")]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> RemoveCoupon()
        {
            var cart = await _cartService.RemoveCouponAsync(UserId);
            return Ok(cart);
        }
    }
}