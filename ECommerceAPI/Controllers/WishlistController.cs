using System.Security.Claims;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var wishlist = await _wishlistService.GetWishlistAsync(UserId);
            return Ok(wishlist);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToWishlistDto dto)
        {
            var wishlist = await _wishlistService.AddItemAsync(UserId, dto);
            return Ok(wishlist);
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var wishlist = await _wishlistService.RemoveItemAsync(UserId, id);
            return Ok(wishlist);
        }

        [HttpDelete("products/{productId}")]
        public async Task<IActionResult> RemoveByProduct(int productId)
        {
            var wishlist = await _wishlistService.RemoveByProductAsync(UserId, productId);
            return Ok(wishlist);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearWishlist()
        {
            var wishlist = await _wishlistService.ClearWishlistAsync(UserId);
            return Ok(wishlist);
        }
    }
}