namespace EstoqueFacil.Application.Dtos
{
    // Usuário logado troca a própria senha.
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
