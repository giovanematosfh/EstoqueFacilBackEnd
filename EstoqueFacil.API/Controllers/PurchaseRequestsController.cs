using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueFacil.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PurchaseRequestsController : ControllerBase
    {
        private readonly IPurchaseRequestService _purchaseRequestService;

        public PurchaseRequestsController(IPurchaseRequestService purchaseRequestService)
        {
            _purchaseRequestService = purchaseRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int branchId, [FromQuery] string? search, [FromQuery] PurchaseRequestStatus? status, [FromQuery] int? productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var purchaseRequests = await _purchaseRequestService.GetPagedAsync(branchId, search, status, productId, page, pageSize);
            return Ok(purchaseRequests);
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport([FromQuery] int? branchId, [FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            var purchaseRequests = await _purchaseRequestService.GetByDateRangeAsync(branchId, from, to);
            return Ok(purchaseRequests);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var purchaseRequest = await _purchaseRequestService.GetByIdAsync(id);
            return Ok(purchaseRequest);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseRequestDto dto)
        {
            var purchaseRequest = await _purchaseRequestService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = purchaseRequest.Id }, purchaseRequest);
        }

        [HttpPost("{id}/receive")]
        public async Task<IActionResult> Receive(int id, [FromBody] ReceivePurchaseRequestDto dto)
        {
            var purchaseRequest = await _purchaseRequestService.ReceiveAsync(id, dto);
            return Ok(purchaseRequest);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var purchaseRequest = await _purchaseRequestService.CancelAsync(id);
            return Ok(purchaseRequest);
        }
    }
}
