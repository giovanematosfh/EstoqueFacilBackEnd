using EstoqueFacil.Application.Constants;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using EstoqueFacil.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.API.Tenancy
{
    // Dados mínimos da plataforma, garantidos a cada inicialização (idempotente).
    public static class PlatformBootstrap
    {
        public static async Task RunAsync(IServiceProvider services, IConfiguration configuration)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var dbContext = provider.GetRequiredService<AppDbContext>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(PlatformBootstrap));

            foreach (var roleName in new[] { Roles.Admin, Roles.User, Roles.SuperAdmin })
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }

            // Fora de uma requisição não há tenant, então os filtros globais são ignorados aqui.
            var companiesWithoutBranch = await dbContext.Companies
                .Where(c => !dbContext.Branches.IgnoreQueryFilters().Any(b => b.CompanyId == c.Id))
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var companyId in companiesWithoutBranch)
            {
                dbContext.Branches.Add(new Branch { CompanyId = companyId, Name = "Matriz", Active = true, CreatedAt = DateTime.UtcNow });
            }

            await dbContext.SaveChangesAsync();

            await EnsureSuperAdminAsync(dbContext, userManager, configuration, logger);
        }

        // Cria o primeiro super admin (dono da plataforma) a partir de SuperAdmin:Email/Password,
        // somente enquanto nenhum existir.
        private static async Task EnsureSuperAdminAsync(
            AppDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ILogger logger)
        {
            if ((await userManager.GetUsersInRoleAsync(Roles.SuperAdmin)).Count > 0)
            {
                return;
            }

            var email = configuration["SuperAdmin:Email"];
            var password = configuration["SuperAdmin:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("Nenhum super admin cadastrado. Configure SuperAdmin:Email e SuperAdmin:Password para criá-lo.");
                return;
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var platformCompanyId = await dbContext.Companies.OrderBy(c => c.Id).Select(c => c.Id).FirstAsync();

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = configuration["SuperAdmin:FullName"] ?? "Super Admin",
                    EmailConfirmed = true,
                    CompanyId = platformCompanyId
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    logger.LogError("Não foi possível criar o super admin: {Errors}", string.Join(" ", result.Errors.Select(e => e.Description)));
                    return;
                }
            }

            foreach (var role in new[] { Roles.Admin, Roles.SuperAdmin })
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }

            logger.LogInformation("Super admin {Email} configurado.", email);
        }
    }
}
