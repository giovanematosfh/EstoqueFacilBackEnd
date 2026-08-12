namespace EstoqueFacil.Application.Dtos
{
    public class StockMovementDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public DateTime MovementDate { get; set; }
        public int StockBalanceAfter { get; set; }
        public string? RequesterName { get; set; }
        public string? Sector { get; set; }
    }
}
