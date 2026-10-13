using EstoqueFacil.Application.Constants;
using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;
using EstoqueFacil.Infrastructure.Data;
using EstoqueFacil.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFacil.API.Controllers
{
    // Gestão das empresas clientes (tenants). Exclusivo do super admin.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITenantProvider _tenantProvider;

        public CompaniesController(AppDbContext context, UserManager<ApplicationUser> userManager, ITenantProvider tenantProvider)
        {
            _context = context;
            _userManager = userManager;
            _tenantProvider = tenantProvider;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var query = _context.Companies.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    EF.Functions.ILike(c.Name, $"%{search}%") ||
                    (c.Document != null && EF.Functions.ILike(c.Document, $"%{search}%")) ||
                    (c.Email != null && EF.Functions.ILike(c.Email, $"%{search}%")));
            }

            var companies = await ProjectToDto(query.OrderBy(c => c.Name)).ToListAsync();
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await GetDtoAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto)
        {
            await EnsurePlanExistsAsync(dto.PlanId);

            if (await _userManager.FindByEmailAsync(dto.AdminEmail) != null)
            {
                throw new BusinessException($"Já existe um usuário com o e-mail '{dto.AdminEmail}'.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var company = new Company
            {
                Name = dto.Name,
                Document = dto.Document,
                Email = dto.Email,
                Phone = dto.Phone,
                PlanId = dto.PlanId,
                DueDate = dto.DueDate,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Toda empresa começa com a filial Matriz.
            _context.Branches.Add(new Branch
            {
                CompanyId = company.Id,
                Name = "Matriz",
                Active = true,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var admin = new ApplicationUser
            {
                UserName = dto.AdminEmail,
                Email = dto.AdminEmail,
                FullName = dto.AdminFullName,
                EmailConfirmed = true,
                CompanyId = company.Id
            };

            var result = await _userManager.CreateAsync(admin, dto.AdminPassword);
            if (!result.Succeeded)
            {
                throw new BusinessException(string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(admin, Roles.Admin);
            await transaction.CommitAsync();

            var created = await GetDtoAsync(company.Id);
            return CreatedAtAction(nameof(GetById), new { id = company.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCompanyDto dto)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == id);
            if (company == null)
            {
                throw new NotFoundException($"Empresa com id {id} não foi encontrada.");
            }

            await EnsurePlanExistsAsync(dto.PlanId);

            if (!dto.Active && _tenantProvider.CompanyId == id)
            {
                throw new BusinessException("Você não pode desativar a sua própria empresa.");
            }

            company.Name = dto.Name;
            company.Document = dto.Document;
            company.Email = dto.Email;
            company.Phone = dto.Phone;
            company.PlanId = dto.PlanId;
            company.DueDate = dto.DueDate;
            company.Active = dto.Active;

            await _context.SaveChangesAsync();

            return Ok(await GetDtoAsync(id));
        }

        private async Task EnsurePlanExistsAsync(int planId)
        {
            if (!await _context.Plans.AnyAsync(p => p.Id == planId))
            {
                throw new NotFoundException($"Plano com id {planId} não foi encontrado.");
            }
        }

        private async Task<CompanyDto> GetDtoAsync(int id)
        {
            var company = await ProjectToDto(_context.Companies.AsNoTracking().Where(c => c.Id == id)).FirstOrDefaultAsync();
            return company ?? throw new NotFoundException($"Empresa com id {id} não foi encontrada.");
        }

        // Contagens atravessam empresas, por isso ignoram o filtro global de tenant.
        private IQueryable<CompanyDto> ProjectToDto(IQueryable<Company> query)
        {
            return query.Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Document = c.Document,
                Email = c.Email,
                Phone = c.Phone,
                Active = c.Active,
                DueDate = c.DueDate,
                CreatedAt = c.CreatedAt,
                PlanId = c.PlanId,
                PlanName = c.Plan.Name,
                UserCount = _context.Users.Count(u => u.CompanyId == c.Id),
                BranchCount = _context.Branches.IgnoreQueryFilters().Count(b => b.CompanyId == c.Id),
                ProductCount = _context.Products.IgnoreQueryFilters().Count(p => p.CompanyId == c.Id)
            });
        }
    }
}
