namespace EstoqueFacil.Domain.Model
{
    // Solicitação de compra (SC) de uma filial, com um ou mais produtos (itens).
    public class PurchaseRequest : ITenantEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public string Number { get; set; } = string.Empty;
        public DateOnly RequestDate { get; set; }
        public DateOnly? ExpectedDeliveryDate { get; set; }

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        public int? SectorId { get; set; }
        public Sector? Sector { get; set; }

        // Ex.: "30/60 dias", "À vista no PIX".
        public string? PaymentTerms { get; set; }

        // Derivado dos itens: Open enquanto houver item pendente; Received se algum foi recebido; Canceled se todos cancelados.
        public PurchaseRequestStatus Status { get; set; }

        public string? RequesterName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();

        public void RefreshStatus()
        {
            if (Items.Any(i => i.Status == PurchaseRequestStatus.Open))
            {
                Status = PurchaseRequestStatus.Open;
            }
            else if (Items.Any(i => i.Status == PurchaseRequestStatus.Received))
            {
                Status = PurchaseRequestStatus.Received;
            }
            else
            {
                Status = PurchaseRequestStatus.Canceled;
            }
        }
    }
}
