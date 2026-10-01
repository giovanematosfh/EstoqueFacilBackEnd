using EstoqueFacil.Application.Dtos;
using EstoqueFacil.Application.Interfaces.Repositories;
using EstoqueFacil.Application.Interfaces.Services;
using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var totalProducts = await _unitOfWork.Products.CountAsync();
            var lowStockCount = await _unitOfWork.ProductStocks.GetLowStockCountAsync();
            var totalStockValue = await _unitOfWork.ProductStocks.GetTotalStockValueAsync();

            var todayStart = DateTime.UtcNow.Date;
            var movementsToday = await _unitOfWork.StockMovements.CountByDateRangeAsync(todayStart, DateTime.UtcNow);

            var recentMovements = await _unitOfWork.StockMovements.GetRecentAsync(5);

            return new DashboardSummaryDto
            {
                TotalProducts = totalProducts,
                LowStockCount = lowStockCount,
                TotalStockValue = totalStockValue,
                MovementsToday = movementsToday,
                RecentMovements = recentMovements.Select(MapMovement)
            };
        }

        private static DashboardMovementDto MapMovement(StockMovement movement)
        {
            return new DashboardMovementDto
            {
                ProductName = movement.Product?.Name ?? string.Empty,
                BranchName = movement.Branch?.Name ?? string.Empty,
                Type = movement.Type.ToString(),
                Quantity = movement.Quantity,
                MovementDate = movement.MovementDate
            };
        }
    }
}
