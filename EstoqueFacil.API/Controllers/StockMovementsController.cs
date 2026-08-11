using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueFacil.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockMovementsController : ControllerBase
    {
        private readonly IStockMovementService _stockMovementService;

        public StockMovementsController(IStockMovementService stockMovementService)
        {
            _stockMovementService = stockMovementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? branchId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var movements = await _stockMovementService.GetAllAsync(search, branchId, page, pageSize);
            return Ok(movements);
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] int? branchId)
        {
            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            var movements = await _stockMovementService.GetByDateRangeAsync(fromUtc, toUtc, branchId);
            return Ok(movements);
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var movements = await _stockMovementService.GetByProductIdAsync(productId);
            return Ok(movements);
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreateStockMovementDto dto)
        {
            var movement = await _stockMovementService.RegisterAsync(dto);
            return CreatedAtAction(nameof(GetByProduct), new { productId = movement.ProductId }, movement);
        }
    }
}
