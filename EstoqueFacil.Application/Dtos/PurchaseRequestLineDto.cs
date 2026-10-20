namespace EstoqueFacil.Application.Dtos
{
    // Um item de SC "achatado" com os dados da SC: base dos relatórios e da visão mensal por produto.
    public class PurchaseRequestLineDto
    {
        public int Id { get; set; }
        public int PurchaseRequestId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Total { get; set; }
        public string? SupplierName { get; set; }
        public string? SectorName { get; set; }
        public DateOnly RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly? ReceivedDate { get; set; }
        public int? ReceivedQuantity { get; set; }
        public string? RequesterName { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
