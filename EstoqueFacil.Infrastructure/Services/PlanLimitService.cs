using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.Infrastructure.Services
{
    public class PlanLimitService : IPlanLimitService
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public PlanLimitService(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task EnsureCanAddUserAsync(int companyId)
        {
            var plan = await GetPlanAsync(companyId);
            if (plan.MaxUsers is not int max)
            {
                return;
            }

            var count = await _context.Users.CountAsync(u => u.CompanyId == companyId);
            if (count >= max)
            {
                throw new BusinessException($"O plano {plan.Name} permite até {max} usuário(s). Faça um upgrade para cadastrar mais.");
            }
        }

        public async Task EnsureCanAddBranchAsync()
        {
            var plan = await GetPlanAsync(CurrentCompanyId());
            if (plan.MaxBranches is not int max)
            {
                return;
            }

            // Branches já vem filtrado pela empresa atual.
            if (await _context.Branches.CountAsync() >= max)
            {
                throw new BusinessException($"O plano {plan.Name} permite até {max} filial(is). Faça um upgrade para cadastrar mais.");
            }
        }

        public async Task EnsureCanAddProductAsync()
        {
            var plan = await GetPlanAsync(CurrentCompanyId());
            if (plan.MaxProducts is not int max)
            {
                return;
            }

            if (await _context.Products.CountAsync() >= max)
            {
                throw new BusinessException($"O plano {plan.Name} permite até {max} produto(s). Faça um upgrade para cadastrar mais.");
            }
        }

        private int CurrentCompanyId()
        {
            return _tenantProvider.CompanyId
                ?? throw new InvalidOperationException("Não há empresa no contexto da requisição.");
        }

        private async Task<Plan> GetPlanAsync(int companyId)
        {
            var plan = await _context.Companies
                .Where(c => c.Id == companyId)
                .Select(c => c.Plan)
                .FirstOrDefaultAsync();

            return plan ?? throw new BusinessException("Empresa sem plano associado.");
        }
    }
}
