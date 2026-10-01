namespace EstoqueFacil.Application.Dtos
{
    public class UpdatePurchaseRequestDto
    {
        public string RequestNumber { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
    }
}
