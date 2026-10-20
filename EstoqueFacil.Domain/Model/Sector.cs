namespace EstoqueFacil.Domain.Model
{
    // Setor da empresa que solicita compras (ex.: Manutenção, Produção, Administrativo).
    public class Sector : ITenantEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ResponsibleName { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
