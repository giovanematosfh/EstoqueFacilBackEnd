namespace EstoqueFacil.Domain.Model
{
    public class ProductStock : ITenantEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public int Quantity { get; set; }
        public int MinimumQuantity { get; set; }
        public string? PurchaseRequestNumber { get; set; }
    }
}
