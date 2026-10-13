namespace EstoqueFacil.Application.Dtos
{
    public class PlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? MaxUsers { get; set; }
        public int? MaxBranches { get; set; }
        public int? MaxProducts { get; set; }
        public bool Active { get; set; }
        public int CompanyCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
