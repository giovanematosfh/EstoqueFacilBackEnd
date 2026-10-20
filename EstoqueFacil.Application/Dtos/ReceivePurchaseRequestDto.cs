namespace EstoqueFacil.Application.Dtos
{
    // Recebimento de um ou mais itens da SC; cada quantidade dá entrada no estoque.
    // Itens não informados (ou com quantidade 0) continuam pendentes.
    public class ReceivePurchaseRequestDto
    {
        public DateOnly ReceivedDate { get; set; }
        public List<ReceivePurchaseRequestItemDto> Items { get; set; } = new();
    }

    public class ReceivePurchaseRequestItemDto
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
