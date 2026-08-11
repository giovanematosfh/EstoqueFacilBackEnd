using System.Security.Claims;
using EstoqueFacil.Application.Constants;
using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueFacil.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = _userManager.Users.OrderBy(u => u.FullName).ToList();
            var result = new List<UserDto>();

            foreach (var user in users)
            {
                result.Add(await MapToDtoAsync(user));
            }

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new NotFoundException($"Usuário com id {id} não foi encontrado.");
            }

            user.FullName = dto.FullName;

            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManager.SetEmailAsync(user, dto.Email);
                if (!emailResult.Succeeded)
                {
                    throw new BusinessException(string.Join(" ", emailResult.Errors.Select(e => e.Description)));
                }

                await _userManager.SetUserNameAsync(user, dto.Email);
                user.EmailConfirmed = true;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new BusinessException(string.Join(" ", updateResult.Errors.Select(e => e.Description)));
            }

            return Ok(await MapToDtoAsync(user));
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateUserRoleDto dto)
        {
            if (dto.Role != Roles.Admin && dto.Role != Roles.User)
            {
                throw new BusinessException("Papel inválido. Use 'Admin' ou 'User'.");
            }

            if (id == GetCurrentUserId())
            {
                throw new BusinessException("Você não pode alterar seu próprio papel.");
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new NotFoundException($"Usuário com id {id} não foi encontrado.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(await MapToDtoAsync(user));
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            if (id == GetCurrentUserId())
            {
                throw new BusinessException("Você não pode desativar sua própria conta.");
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new NotFoundException($"Usuário com id {id} não foi encontrado.");
            }

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, dto.IsActive ? null : DateTimeOffset.MaxValue);

            return Ok(await MapToDtoAsync(user));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            if (id == GetCurrentUserId())
            {
                throw new BusinessException("Você não pode excluir sua própria conta.");
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new NotFoundException($"Usuário com id {id} não foi encontrado.");
            }

            await _userManager.DeleteAsync(user);
            return NoContent();
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.Parse(idClaim!);
        }

        private async Task<UserDto> MapToDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isActive = !await _userManager.IsLockedOutAsync(user);

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Role = roles.FirstOrDefault() ?? Roles.User,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = isActive
            };
        }
    }
}
