namespace Application.Interfaces;

public interface IPaymentProcessedPublisher
{
    Task PublishPaymentProcessedAsync(int idUsuario, int idCompra, bool aprovado);
}