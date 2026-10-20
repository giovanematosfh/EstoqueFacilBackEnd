namespace EstoqueFacil.Application.Dtos
{
    // SC com seus itens (tela de Compras).
    public class PurchaseRequestDto
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public DateOnly RequestDate { get; set; }
        public DateOnly? ExpectedDeliveryDate { get; set; }
        public int? SectorId { get; set; }
        public string? SectorName { get; set; }
        public string? PaymentTerms { get; set; }
        public PurchaseRequestSupplierDto? Supplier { get; set; }

        // Soma dos itens com preço informado.
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RequesterName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PurchaseRequestItemDto> Items { get; set; } = new();
    }

    public class PurchaseRequestItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly? ReceivedDate { get; set; }
        public int? ReceivedQuantity { get; set; }
    }

    // Dados do fornecedor que vão para a ordem de compra.
    public class PurchaseRequestSupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Document { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
