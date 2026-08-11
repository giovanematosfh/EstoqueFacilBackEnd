namespace EstoqueFacil.Application.Dtos
{
    public class UpdateBranchDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool Active { get; set; }
    }
}
