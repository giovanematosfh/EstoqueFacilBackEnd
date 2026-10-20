namespace EstoqueFacil.Domain.Model
{
    // Produto solicitado em uma SC. Cada item é recebido (ou cancelado) separadamente.
    public class PurchaseRequestItem : ITenantEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public int PurchaseRequestId { get; set; }
        public PurchaseRequest PurchaseRequest { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public PurchaseRequestStatus Status { get; set; }
        public DateOnly? ReceivedDate { get; set; }
        public int? ReceivedQuantity { get; set; }
    }
}
