using System.Text.Json;
using EstoqueFacil.Application.Constants;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.API.Middlewares
{
    // Bloqueia requisições autenticadas de empresas desativadas ou com assinatura vencida.
    // O super admin nunca é bloqueado, para poder renovar as assinaturas.
    public class CompanyStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public CompanyStatusMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ITenantProvider tenantProvider, AppDbContext dbContext)
        {
            if (context.User.Identity?.IsAuthenticated != true || context.User.IsInRole(Roles.SuperAdmin))
            {
                await _next(context);
                return;
            }

            if (tenantProvider.CompanyId is not int companyId)
            {
                // Token antigo, emitido antes do multiempresa: força novo login.
                await WriteAsync(context, StatusCodes.Status401Unauthorized, "Sessão expirada. Entre novamente.");
                return;
            }

            var company = await dbContext.Companies.AsNoTracking()
                .Where(c => c.Id == companyId)
                .Select(c => new { c.Active, c.DueDate })
                .FirstOrDefaultAsync();

            var blockedMessage = GetBlockedMessage(company?.Active ?? false, company?.DueDate);
            if (blockedMessage != null)
            {
                // 402 diferencia "empresa bloqueada" de um 403 comum de permissão.
                await WriteAsync(context, StatusCodes.Status402PaymentRequired, blockedMessage);
                return;
            }

            await _next(context);
        }

        public static string? GetBlockedMessage(bool active, DateOnly? dueDate)
        {
            if (!active)
            {
                return "O acesso da sua empresa está desativado. Entre em contato com o suporte.";
            }

            if (dueDate is DateOnly due && due < DateOnly.FromDateTime(DateTime.Today))
            {
                return $"A assinatura da sua empresa venceu em {due:dd/MM/yyyy}. Entre em contato para renovar.";
            }

            return null;
        }

        private static Task WriteAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(new { mensagem = message }));
        }
    }
}
