namespace Entities;

public class CompraJogo
{
    public int CompraId { get; private set; }
    public int JogoId { get; private set; }
    public decimal PrecoAplicado { get; private set; }

    protected CompraJogo() { }

    public CompraJogo(int jogoId, decimal preco)
    {
        JogoId = jogoId;
        PrecoAplicado = preco;
    }
}