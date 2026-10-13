namespace EstoqueFacil.Domain.Model
{
    // Entidades que pertencem a uma empresa (tenant). O AppDbContext filtra e preenche CompanyId automaticamente.
    public interface ITenantEntity
    {
        int CompanyId { get; set; }
    }
}
