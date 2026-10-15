namespace EstoqueFacil.Application.Dtos
{
    public class CreatePurchaseRequestDto
    {
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public string Number { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateOnly RequestDate { get; set; }
        public string? RequesterName { get; set; }
        public string? Notes { get; set; }
    }
}
