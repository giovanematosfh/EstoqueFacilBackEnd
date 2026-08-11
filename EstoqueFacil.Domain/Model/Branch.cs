namespace EstoqueFacil.Domain.Model
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }
}
