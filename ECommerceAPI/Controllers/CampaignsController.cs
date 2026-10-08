using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public CampaignsController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var campaigns = await _campaignService.GetAllAsync();

        return Ok(campaigns);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var campaign = await _campaignService.GetByIdAsync(id);

        if (campaign == null)
            return NotFound(new
            {
                Message = "Campaign not found."
            });

        return Ok(campaign);
    }

    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetActiveForProduct(int productId)
    {
        var campaign =
            await _campaignService.GetActiveCampaignForProductAsync(productId);

        if (campaign == null)
            return NotFound(new
            {
                Message = "No active campaign found for this product."
            });

        return Ok(campaign);
    }
}