namespace EstoqueFacil.Application.Dtos
{
    // Usado para criar e para editar (enquanto nenhum item foi recebido) a SC.
    public class CreatePurchaseRequestDto
    {
        public int BranchId { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateOnly RequestDate { get; set; }
        public DateOnly? ExpectedDeliveryDate { get; set; }
        public int? SupplierId { get; set; }
        public int? SectorId { get; set; }
        public string? PaymentTerms { get; set; }
        public string? RequesterName { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseRequestItemDto> Items { get; set; } = new();
    }

    public class CreatePurchaseRequestItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
    }
}
