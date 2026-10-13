namespace EstoqueFacil.Application.Interfaces.Services
{
    // Empresa (tenant) do usuário autenticado na requisição atual; null quando anônimo.
    public interface ITenantProvider
    {
        int? CompanyId { get; }
    }
}
