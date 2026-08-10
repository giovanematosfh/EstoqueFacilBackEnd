namespace EstoqueFacil.Application.Interfaces.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(int userId, string email, string fullName, IList<string> roles);
    }
}
