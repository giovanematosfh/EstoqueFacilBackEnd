namespace EstoqueFacil.Application.Dtos
{
    public class CreatePurchaseRequestDto
    {
        public int BranchId { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateOnly RequestDate { get; set; }
        public string? RequesterName { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseRequestItemDto> Items { get; set; } = new();
    }

    public class CreatePurchaseRequestItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
