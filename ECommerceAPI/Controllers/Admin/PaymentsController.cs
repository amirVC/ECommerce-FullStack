using ECommerceAPI.DTOs;
using ECommerceAPI.QueryParameters;
using ECommerceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class PaymentsController : ControllerBase
    {
        private readonly IAdminPaymentService _paymentService;

        public PaymentsController(IAdminPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET api/admin/payments?status=1&provider=0&page=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<AdminPaymentHistoryResultDto>> Get([FromQuery] PaymentQueryParameters query)
        {
            var result = await _paymentService.GetAllAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminPaymentDto>> GetById(int id)
        {
            var payment = await _paymentService.GetByIdAsync(id);

            if (payment == null)
                return NotFound();

            return Ok(payment);
        }
    }
}
