namespace EstoqueFacil.Domain.Model
{
    public class Category : ITenantEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
