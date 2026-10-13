namespace EstoqueFacil.Domain.Model
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // null = ilimitado
        public int? MaxUsers { get; set; }
        public int? MaxBranches { get; set; }
        public int? MaxProducts { get; set; }

        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Company> Companies { get; set; } = new List<Company>();
    }
}
