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

        // ==========================================
        // GET CART
        // Global limit: 100/minute
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cart = await _cartService.GetCartAsync(UserId);
            return Ok(cart);
        }

        // ==========================================
        // ADD ITEM
        // Cart limit: 30/minute
        // ==========================================
        [HttpPost("items")]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> AddItem(
            [FromBody] AddToCartDto dto)
        {
            var cart = await _cartService.AddItemAsync(UserId, dto);
            return Ok(cart);
        }

        // ==========================================
        // UPDATE ITEM
        // Cart limit: 30/minute
        // ==========================================
        [HttpPut("items/{id}")]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> UpdateItem(
            int id,
            [FromBody] UpdateCartItemDto dto)
        {
            var cart = await _cartService.UpdateItemAsync(
                UserId,
                id,
                dto);

            return Ok(cart);
        }

        // ==========================================
        // REMOVE ITEM
        // Cart limit: 30/minute
        // ==========================================
        [HttpDelete("items/{id}")]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var cart = await _cartService.RemoveItemAsync(
                UserId,
                id);

            return Ok(cart);
        }

        // ==========================================
        // CLEAR CART
        // Cart limit: 30/minute
        // ==========================================
        [HttpDelete]
        [EnableRateLimiting("cart")]
        public async Task<IActionResult> ClearCart()
        {
            var cart = await _cartService.ClearCartAsync(UserId);
            return Ok(cart);
        }

        // ==========================================
        // APPLY COUPON
        // Cart limit: 30/minute
        // ==========================================
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

        // ==========================================
        // REMOVE COUPON
        // Global limit: 100/minute
        // ==========================================
        [HttpDelete("coupon")]
        public async Task<IActionResult> RemoveCoupon()
        {
            var cart = await _cartService.RemoveCouponAsync(UserId);
            return Ok(cart);
        }
    }
}
