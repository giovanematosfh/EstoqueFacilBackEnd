using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Exceptions;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SupplierService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<SupplierDto>> GetAllAsync(string? search, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var (suppliers, totalCount) = await _unitOfWork.Suppliers.GetPagedAsync(search, page, pageSize);

            return new PagedResultDto<SupplierDto>
            {
                Items = suppliers.Select(MapToDto),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<SupplierDto> GetByIdAsync(int id)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (supplier == null)
            {
                throw new NotFoundException($"Fornecedor com id {id} não foi encontrado.");
            }

            return MapToDto(supplier);
        }

        public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto)
        {
            var supplier = new Supplier
            {
                Name = dto.Name,
                Document = dto.Document,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Suppliers.Add(supplier);
            await _unitOfWork.CommitAsync();

            return MapToDto(supplier);
        }

        public async Task<SupplierDto> UpdateAsync(int id, UpdateSupplierDto dto)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (supplier == null)
            {
                throw new NotFoundException($"Fornecedor com id {id} não foi encontrado.");
            }

            supplier.Name = dto.Name;
            supplier.Document = dto.Document;
            supplier.Email = dto.Email;
            supplier.Phone = dto.Phone;
            supplier.Address = dto.Address;

            await _unitOfWork.CommitAsync();

            return MapToDto(supplier);
        }

        public async Task RemoveAsync(int id)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (supplier == null)
            {
                throw new NotFoundException($"Fornecedor com id {id} não foi encontrado.");
            }

            _unitOfWork.Suppliers.Remove(supplier);
            await _unitOfWork.CommitAsync();
        }

        private static SupplierDto MapToDto(Supplier supplier)
        {
            return new SupplierDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Document = supplier.Document,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address,
                CreatedAt = supplier.CreatedAt
            };
        }
    }
}
