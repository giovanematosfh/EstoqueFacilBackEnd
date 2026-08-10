namespace EstoqueFacil.Application.Dtos
{
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStockQuantity { get; set; }
        public int CategoryId { get; set; }
    }
}
