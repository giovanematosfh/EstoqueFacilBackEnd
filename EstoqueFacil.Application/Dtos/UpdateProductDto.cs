namespace EstoqueFacil.Application.Dtos
{
    public class UpdateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool Active { get; set; }
        public int CategoryId { get; set; }
    }
}
