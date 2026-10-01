namespace EstoqueFacil.Application.Dtos
{
    public class UpdateStockMovementDto
    {
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public string? RequesterName { get; set; }
        public string? Sector { get; set; }
    }
}
