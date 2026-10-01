using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Interfaces.Services;
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
        public async Task<IActionResult> GetAll([FromQuery] int branchId, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var purchaseRequests = await _purchaseRequestService.GetPagedByBranchAsync(branchId, search, page, pageSize);
            return Ok(purchaseRequests);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseRequestDto dto)
        {
            var purchaseRequest = await _purchaseRequestService.CreateAsync(dto);
            return Ok(purchaseRequest);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseRequestDto dto)
        {
            var purchaseRequest = await _purchaseRequestService.UpdateAsync(id, dto);
            return Ok(purchaseRequest);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            await _purchaseRequestService.RemoveAsync(id);
            return NoContent();
        }
    }
}
