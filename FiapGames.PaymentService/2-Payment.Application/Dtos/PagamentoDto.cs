using Domain;

namespace _2_Payment.Application.Dtos
{
    public record PagamentoDto(
        int IdPagamento,
        int IdCompra,
        DateTime DataHoraInclusao,
        DateTime? DataHoraAlteracao,
        STATUS_PAGAMENTO Status,
        int IdUsuario
    );
}
