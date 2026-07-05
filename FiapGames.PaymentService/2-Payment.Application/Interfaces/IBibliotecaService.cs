namespace Application.Interfaces;

public interface IBibliotecaService
{
    Task AdicionarJogo(int idUsuario, int idJogo);
}