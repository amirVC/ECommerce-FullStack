using System.Security.Claims;
using ECommerceAPI.DTOs.RecentlyViewed;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/recently-viewed")]
[Authorize]
public class RecentlyViewedController : ControllerBase
{
    private readonly IRecentlyViewedService _recentlyViewedService;

    public RecentlyViewedController(IRecentlyViewedService recentlyViewedService)
    {
        _recentlyViewedService = recentlyViewedService;
    }

    [HttpPost]
    public async Task<IActionResult> Add(CreateRecentlyViewedDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _recentlyViewedService.AddAsync(userId, dto.ProductId);

        return NoContent();
    }
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var products = await _recentlyViewedService.GetRecentlyViewedAsync(userId);

        return Ok(products);
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _recentlyViewedService.ClearAsync(userId);

        return NoContent();
    }
}