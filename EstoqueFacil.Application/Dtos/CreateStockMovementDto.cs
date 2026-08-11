using EstoqueFacil.Domain.Model;

namespace EstoqueFacil.Application.Dtos
{
    public class CreateStockMovementDto
    {
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public MovementType Type { get; set; }
        public int Quantity { get; set; }
        public string? Reason { get; set; }
    }
}
