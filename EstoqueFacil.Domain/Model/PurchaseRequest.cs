namespace EstoqueFacil.Domain.Model
{
    // Solicitação de compra (SC) feita para repor o estoque de um produto em uma filial.
    public class PurchaseRequest : ITenantEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public string Number { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateOnly RequestDate { get; set; }
        public PurchaseRequestStatus Status { get; set; }
        public DateOnly? ReceivedDate { get; set; }
        public int? ReceivedQuantity { get; set; }
        public string? RequesterName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
