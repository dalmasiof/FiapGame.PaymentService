namespace IntegrationEvents;

public record NotificacaoIntegrationEvent(
    Guid CorrelacaoId,
    string Destinatario,
    string Assunto,
    string CorpoMensagem,
    string DominioOrigem)
{
    public string RastreioId { get; init; } = string.Empty;
}
