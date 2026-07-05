using Enum;

namespace DTOs;
public record PagamentoDto(
    int IdPagamento,
    int IdCompra,
    DateTime DataHoraInclusao,
    DateTime? DataHoraAlteracao,
    STATUS_PAGAMENTO Status,
    int IdUsuario
);