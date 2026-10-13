namespace EstoqueFacil.Application.Dtos
{
    public class UpdateCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Document { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int PlanId { get; set; }
        public DateOnly? DueDate { get; set; }
        public bool Active { get; set; }
    }
}
