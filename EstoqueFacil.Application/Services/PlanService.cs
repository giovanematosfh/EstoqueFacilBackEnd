using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Services
{
    public class PlanService : IPlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<IEnumerable<PlanDto>> GetAllAsync()
        {
            var plans = _unitOfWork.Plans.Query()
                .OrderBy(p => p.Price)
                .Select(p => new PlanDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    MaxUsers = p.MaxUsers,
                    MaxBranches = p.MaxBranches,
                    MaxProducts = p.MaxProducts,
                    Active = p.Active,
                    CompanyCount = p.Companies.Count,
                    CreatedAt = p.CreatedAt
                })
                .ToList();

            return Task.FromResult<IEnumerable<PlanDto>>(plans);
        }

        public async Task<PlanDto> GetByIdAsync(int id)
        {
            var plan = await GetPlanAsync(id);
            return MapToDto(plan, CountCompanies(id));
        }

        public async Task<PlanDto> CreateAsync(SavePlanDto dto)
        {
            EnsureUniqueName(dto.Name);

            var plan = new Plan { CreatedAt = DateTime.UtcNow };
            Apply(plan, dto);

            _unitOfWork.Plans.Add(plan);
            await _unitOfWork.CommitAsync();

            return MapToDto(plan, 0);
        }

        public async Task<PlanDto> UpdateAsync(int id, SavePlanDto dto)
        {
            var plan = await GetPlanAsync(id);
            EnsureUniqueName(dto.Name, id);

            Apply(plan, dto);
            await _unitOfWork.CommitAsync();

            return MapToDto(plan, CountCompanies(id));
        }

        public async Task RemoveAsync(int id)
        {
            var plan = await GetPlanAsync(id);

            if (CountCompanies(id) > 0)
            {
                throw new BusinessException("Este plano está em uso por empresas. Desative-o em vez de excluir.");
            }

            _unitOfWork.Plans.Remove(plan);
            await _unitOfWork.CommitAsync();
        }

        private async Task<Plan> GetPlanAsync(int id)
        {
            var plan = await _unitOfWork.Plans.GetByIdAsync(id);
            if (plan == null)
            {
                throw new NotFoundException($"Plano com id {id} não foi encontrado.");
            }

            return plan;
        }

        private void EnsureUniqueName(string name, int? ignoreId = null)
        {
            var exists = _unitOfWork.Plans.Query()
                .Any(p => p.Name.ToLower() == name.ToLower() && (!ignoreId.HasValue || p.Id != ignoreId.Value));

            if (exists)
            {
                throw new BusinessException($"Já existe um plano com o nome '{name}'.");
            }
        }

        private int CountCompanies(int planId)
        {
            return _unitOfWork.Companies.Query().Count(c => c.PlanId == planId);
        }

        private static void Apply(Plan plan, SavePlanDto dto)
        {
            plan.Name = dto.Name;
            plan.Price = dto.Price;
            plan.MaxUsers = dto.MaxUsers;
            plan.MaxBranches = dto.MaxBranches;
            plan.MaxProducts = dto.MaxProducts;
            plan.Active = dto.Active;
        }

        private static PlanDto MapToDto(Plan plan, int companyCount)
        {
            return new PlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Price = plan.Price,
                MaxUsers = plan.MaxUsers,
                MaxBranches = plan.MaxBranches,
                MaxProducts = plan.MaxProducts,
                Active = plan.Active,
                CompanyCount = companyCount,
                CreatedAt = plan.CreatedAt
            };
        }
    }
}
