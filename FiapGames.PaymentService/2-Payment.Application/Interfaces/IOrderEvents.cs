namespace Application.Interfaces;

public interface IOrderEvents
{
    Task HandleOrderPlacedAsync(int idUsuario, int idCompra);
}