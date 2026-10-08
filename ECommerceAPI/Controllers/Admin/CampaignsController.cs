using ECommerceAPI.DTOs;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
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

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] AdminCreateCampaignDto dto)
    {
        var campaign = await _campaignService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = campaign.Id },
            campaign);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] AdminUpdateCampaignDto dto)
    {
        var campaign = await _campaignService.UpdateAsync(id, dto);

        if (campaign == null)
            return NotFound(new
            {
                Message = "Campaign not found."
            });

        return Ok(campaign);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _campaignService.DeleteAsync(id);

        if (!deleted)
            return NotFound(new
            {
                Message = "Campaign not found."
            });

        return Ok(new
        {
            Message = "Campaign deleted successfully."
        });
    }
}