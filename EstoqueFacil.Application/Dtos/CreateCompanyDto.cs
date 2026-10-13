namespace EstoqueFacil.Application.Dtos
{
    // Cria a empresa já com o usuário administrador dela.
    public class CreateCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Document { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int PlanId { get; set; }
        public DateOnly? DueDate { get; set; }

        public string AdminFullName { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminPassword { get; set; } = string.Empty;
    }
}
