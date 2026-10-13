namespace EstoqueFacil.Application.Interfaces.Services
{
    // Lança BusinessException quando o plano da empresa não permite mais cadastros.
    public interface IPlanLimitService
    {
        Task EnsureCanAddUserAsync(int companyId);
        Task EnsureCanAddBranchAsync();
        Task EnsureCanAddProductAsync();
    }
}
