using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueFacil.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductStocksController : ControllerBase
    {
        private readonly IProductStockService _productStockService;

        public ProductStocksController(IProductStockService productStockService)
        {
            _productStockService = productStockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int branchId, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var productStocks = await _productStockService.GetPagedByBranchAsync(branchId, search, page, pageSize);
            return Ok(productStocks);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] int branchId)
        {
            var productStocks = await _productStockService.GetLowStockAsync(branchId);
            return Ok(productStocks);
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetReport([FromQuery] int branchId)
        {
            var productStocks = await _productStockService.GetAllByBranchAsync(branchId);
            return Ok(productStocks);
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateMinimum(int productId, [FromQuery] int branchId, [FromBody] UpdateProductStockDto dto)
        {
            var productStock = await _productStockService.UpdateMinimumAsync(productId, branchId, dto);
            return Ok(productStock);
        }
    }
}
