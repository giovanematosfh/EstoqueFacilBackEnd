namespace EstoqueFacil.Application.Dtos
{
    public class PurchaseRequestDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
