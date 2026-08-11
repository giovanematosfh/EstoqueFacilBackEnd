using System.Net;
using EstoqueFacil.Application.Constants;
using EstoqueFacil.Application.Dtos.Auth;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueFacil.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new BusinessException(errors);
            }

            // TODO: reativar quando o SMTP (appsettings.json -> Smtp:Password) estiver configurado corretamente.
            // try
            // {
            //     await SendConfirmationEmailAsync(user);
            // }
            // catch (Exception)
            // {
            //     await _userManager.DeleteAsync(user);
            //     throw new BusinessException(
            //         "Não foi possível enviar o e-mail de confirmação. Tente novamente em instantes.");
            // }
            //
            // return Ok(new RegisterResponseDto
            // {
            //     Message = "Cadastro realizado! Verifique seu e-mail para confirmar a conta antes de entrar."
            // });

            await _userManager.ConfirmEmailAsync(user, await _userManager.GenerateEmailConfirmationTokenAsync(user));
            await _userManager.AddToRoleAsync(user, Roles.User);

            return Ok(await BuildAuthResponseAsync(user));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                throw new BusinessException("E-mail ou senha inválidos.");
            }

            // TODO: reativar quando o SMTP estiver configurado corretamente.
            // if (!user.EmailConfirmed)
            // {
            //     throw new BusinessException("Confirme seu e-mail antes de entrar. Verifique sua caixa de entrada.");
            // }

            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new BusinessException("Sua conta foi desativada. Entre em contato com um administrador.");
            }

            return Ok(await BuildAuthResponseAsync(user));
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new NotFoundException("Usuário não encontrado.");
            }

            if (user.EmailConfirmed)
            {
                return Ok(new { mensagem = "E-mail já confirmado." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                throw new BusinessException("Link de confirmação inválido ou expirado.");
            }

            return Ok(new { mensagem = "E-mail confirmado com sucesso." });
        }

        private async Task SendConfirmationEmailAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebUtility.UrlEncode(token);
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"];
            var confirmationLink = $"{frontendBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

            var html = $"""
                <p>Olá, {WebUtility.HtmlEncode(user.FullName)}!</p>
                <p>Confirme seu e-mail para ativar sua conta no Estoque Fácil:</p>
                <p><a href="{confirmationLink}">Confirmar e-mail</a></p>
                <p>Se você não criou essa conta, pode ignorar este e-mail.</p>
                """;

            await _emailSender.SendAsync(user.Email!, "Confirme seu e-mail - Estoque Fácil", html);
        }

        private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var (token, expiresAt) = _tokenService.GenerateToken(user.Id, user.Email!, user.FullName, roles);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = new AuthUserDto
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Email = user.Email!,
                    Role = roles.FirstOrDefault() ?? Roles.User
                }
            };
        }
    }
}
