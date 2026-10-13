using EstoqueFacil.Application.Constants;
using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueFacil.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _planService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _planService.GetByIdAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SavePlanDto dto)
        {
            var plan = await _planService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SavePlanDto dto)
        {
            return Ok(await _planService.UpdateAsync(id, dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            await _planService.RemoveAsync(id);
            return NoContent();
        }
    }
}
