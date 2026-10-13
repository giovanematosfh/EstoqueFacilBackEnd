namespace EstoqueFacil.Domain.Model
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Document { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool Active { get; set; }

        // Data até quando a assinatura está paga (null = sem vencimento).
        public DateOnly? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public int PlanId { get; set; }
        public Plan Plan { get; set; } = null!;
    }
}
