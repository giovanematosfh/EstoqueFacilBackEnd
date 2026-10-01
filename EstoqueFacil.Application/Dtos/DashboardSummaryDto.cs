namespace EstoqueFacil.Application.Dtos
{
    public class DashboardSummaryDto
    {
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public decimal TotalStockValue { get; set; }
        public int MovementsToday { get; set; }
        public IEnumerable<DashboardMovementDto> RecentMovements { get; set; } = Enumerable.Empty<DashboardMovementDto>();
    }
}
