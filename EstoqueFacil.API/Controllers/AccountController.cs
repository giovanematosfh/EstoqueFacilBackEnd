using System.Security.Claims;
using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueFacil.API.Controllers
{
    // Ações do próprio usuário logado (qualquer papel).
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var user = await _userManager.FindByIdAsync(idClaim!);
            if (user == null)
            {
                throw new NotFoundException("Usuário não encontrado.");
            }

            if (!await _userManager.CheckPasswordAsync(user, dto.CurrentPassword))
            {
                throw new BusinessException("A senha atual está incorreta.");
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                throw new BusinessException(string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            return NoContent();
        }
    }
}
