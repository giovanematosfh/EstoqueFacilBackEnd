namespace EstoqueFacil.Domain.Model
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public ICollection<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
    }
}
