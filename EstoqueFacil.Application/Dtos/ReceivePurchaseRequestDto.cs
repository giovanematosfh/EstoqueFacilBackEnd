namespace EstoqueFacil.Application.Dtos
{
    public class ReceivePurchaseRequestDto
    {
        // Quantidade que chegou; dá entrada no estoque.
        public int Quantity { get; set; }
        public DateOnly ReceivedDate { get; set; }
    }
}
