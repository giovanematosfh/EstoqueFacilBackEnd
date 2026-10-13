using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueFacil.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var products = await _productService.GetAllAsync(search, page, pageSize);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var product = await _productService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            var product = await _productService.UpdateAsync(id, dto);
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            await _productService.RemoveAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/image")]
        [RequestSizeLimit(MaxImageBytes + 1024 * 1024)]
        public async Task<IActionResult> UploadImage(int id, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BusinessException("Selecione uma imagem.");
            }

            if (file.Length > MaxImageBytes)
            {
                throw new BusinessException("A imagem deve ter no máximo 5 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageTypes.TryGetValue(extension, out var contentType) || file.ContentType != contentType)
            {
                throw new BusinessException("Formato inválido. Envie uma imagem JPG, PNG ou WEBP.");
            }

            await using var stream = file.OpenReadStream();
            var product = await _productService.SetImageAsync(id, stream, extension == ".jpeg" ? ".jpg" : extension);
            return Ok(product);
        }

        [HttpDelete("{id}/image")]
        public async Task<IActionResult> RemoveImage(int id)
        {
            var product = await _productService.RemoveImageAsync(id);
            return Ok(product);
        }

        private const long MaxImageBytes = 5 * 1024 * 1024;

        private static readonly Dictionary<string, string> AllowedImageTypes = new()
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };
    }
}
