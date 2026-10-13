namespace EstoqueFacil.Application.Dtos
{
    public class SavePlanDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? MaxUsers { get; set; }
        public int? MaxBranches { get; set; }
        public int? MaxProducts { get; set; }
        public bool Active { get; set; } = true;
    }
}
