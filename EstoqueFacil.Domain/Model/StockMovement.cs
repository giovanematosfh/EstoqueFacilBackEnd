namespace EstoqueFacil.Domain.Model
{
    public class StockMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
        public MovementType Type { get; set; }
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public DateTime MovementDate { get; set; }
        public string? RequesterName { get; set; }
        public string? Sector { get; set; }
    }
}
