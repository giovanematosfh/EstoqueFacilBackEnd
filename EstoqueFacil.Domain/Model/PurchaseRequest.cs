namespace EstoqueFacil.Domain.Model
{
    public class PurchaseRequest
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public string RequestNumber { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
