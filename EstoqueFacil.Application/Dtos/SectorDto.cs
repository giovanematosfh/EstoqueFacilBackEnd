namespace EstoqueFacil.Application.Dtos
{
    public class SectorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ResponsibleName { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SaveSectorDto
    {
        public string Name { get; set; } = string.Empty;
        public string? ResponsibleName { get; set; }
        public bool Active { get; set; } = true;
    }
}
