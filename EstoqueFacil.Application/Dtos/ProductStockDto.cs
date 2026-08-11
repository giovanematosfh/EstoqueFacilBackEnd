namespace EstoqueFacil.Application.Dtos
{
    public class ProductStockDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinimumQuantity { get; set; }
        public bool LowStock => Quantity <= MinimumQuantity;
    }
}
