namespace IntegrationEvents;

public record CompraSolicitadaIntegrationEvent(
    int CompraId,
    int UsuarioId,
    IReadOnlyCollection<int> JogosIds,
    decimal ValorTotal,
    DateTime SolicitadaEm,
    string RastreioId,
    string? EmailUsuario = null,
    string? DestinatarioNotificacao = null);
