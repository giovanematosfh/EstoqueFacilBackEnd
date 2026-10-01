namespace EstoqueFacil.Application.Dtos
{
    public class DashboardMovementDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime MovementDate { get; set; }
    }
}
