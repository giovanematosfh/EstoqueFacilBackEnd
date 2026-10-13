namespace EstoqueFacil.Application.Dtos
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Document { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool Active { get; set; }
        public DateOnly? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int BranchCount { get; set; }
        public int ProductCount { get; set; }
    }
}
