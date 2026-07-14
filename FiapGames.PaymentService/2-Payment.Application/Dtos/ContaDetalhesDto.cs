namespace _2_Payment.Application.Dtos
{
    public record ContaDetalhesDto
    (
        int IdConta,
        int IdLogin,
        decimal Saldo,
        DateTime DataAtualizacao
    );
}