namespace EstoqueFacil.Application.Dtos
{
    public class BranchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
