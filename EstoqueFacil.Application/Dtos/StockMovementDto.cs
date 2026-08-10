namespace EstoqueFacil.Application.Dtos
{
    public class StockMovementDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public DateTime MovementDate { get; set; }
        public int StockBalanceAfter { get; set; }
    }
}
