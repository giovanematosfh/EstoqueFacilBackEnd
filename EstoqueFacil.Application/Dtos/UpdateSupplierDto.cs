namespace EstoqueFacil.Application.Dtos
{
    public class UpdateSupplierDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Document { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
