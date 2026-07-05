namespace IntegrationEvents;
public record ItemEstoqueDto(Guid ProdutoId, int Quantidade);

public record PagamentoAprovadoIntegrationEvent(
    Guid PedidoId,
    decimal ValorPago,
    List<ItemEstoqueDto> Itens,
    string MetodoPagamento
)
{
    public string RastreioId { get; } = Guid.NewGuid().ToString();
}