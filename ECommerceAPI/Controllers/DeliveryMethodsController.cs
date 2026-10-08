using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/delivery-methods")]
public class DeliveryMethodsController : ControllerBase
{
    private readonly IDeliveryMethodService _service;
    public DeliveryMethodsController(IDeliveryMethodService service) => _service = service;

    [HttpGet]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetActive() => Ok(await _service.GetActiveAsync());
}