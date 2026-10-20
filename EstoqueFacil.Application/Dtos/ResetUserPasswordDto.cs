namespace EstoqueFacil.Application.Dtos
{
    // Admin da empresa define uma nova senha para um usuário dela.
    public class ResetUserPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
