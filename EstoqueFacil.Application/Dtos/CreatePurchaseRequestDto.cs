namespace EstoqueFacil.Application.Dtos
{
    public class CreatePurchaseRequestDto
    {
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
    }
}
