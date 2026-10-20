using System.Security.Claims;
using EstoqueFacil.Application.Constants;
using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.API.Controllers
{
    // Usuários da empresa do admin logado; nunca enxerga usuários de outras empresas.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantProvider _tenantProvider;
        private readonly IPlanLimitService _planLimits;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            ITenantProvider tenantProvider,
            IPlanLimitService planLimits)
        {
            _userManager = userManager;
            _tenantProvider = tenantProvider;
            _planLimits = planLimits;
        }

        private int CompanyId => _tenantProvider.CompanyId
            ?? throw new InvalidOperationException("Não há empresa no contexto da requisição.");

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users
                .Where(u => u.CompanyId == CompanyId)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var result = new List<UserDto>();

            foreach (var user in users)
            {
                result.Add(await MapToDtoAsync(user));
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            await _planLimits.EnsureCanAddUserAsync(CompanyId);

            if (await _userManager.FindByEmailAsync(dto.Email) != null)
            {
                throw new BusinessException($"Já existe um usuário com o e-mail '{dto.Email}'.");
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                EmailConfirmed = true,
                CompanyId = CompanyId
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                throw new BusinessException(string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(await MapToDtoAsync(user));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await FindCompanyUserAsync(id);

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

            var user = await FindCompanyUserAsync(id);

            // Troca só Admin/User; o papel de SuperAdmin não é gerenciado por aqui.
            var currentRoles = (await _userManager.GetRolesAsync(user))
                .Where(r => r == Roles.Admin || r == Roles.User)
                .ToList();

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

            var user = await FindCompanyUserAsync(id);

            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, dto.IsActive ? null : DateTimeOffset.MaxValue);

            return Ok(await MapToDtoAsync(user));
        }

        // O admin define a nova senha sem precisar saber a atual (ex.: usuário esqueceu).
        [HttpPut("{id}/password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetUserPasswordDto dto)
        {
            var user = await FindCompanyUserAsync(id);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!result.Succeeded)
            {
                throw new BusinessException(string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            if (id == GetCurrentUserId())
            {
                throw new BusinessException("Você não pode excluir sua própria conta.");
            }

            var user = await FindCompanyUserAsync(id);

            await _userManager.DeleteAsync(user);
            return NoContent();
        }

        private async Task<ApplicationUser> FindCompanyUserAsync(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.CompanyId == CompanyId);
            return user ?? throw new NotFoundException($"Usuário com id {id} não foi encontrado.");
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
                Role = roles.Contains(Roles.Admin) ? Roles.Admin : Roles.User,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = isActive
            };
        }
    }
}
