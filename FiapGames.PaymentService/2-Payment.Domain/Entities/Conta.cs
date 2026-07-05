namespace Entities;
public class Conta
{
    public int IdConta { get; set; }
    public int IdLogin { get; set; }
    public decimal Saldo { get; private set; }
    public DateTime DataAtualizacao { get; private set; }

    public Conta() { }
    public Conta(decimal saldoInicial = 0)
    {
        Saldo = saldoInicial;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Adicionar(decimal valor)
    {
        if (valor <= 0) throw new ArgumentException("Valor a ser adicionado deve ser maior que zero.", nameof(valor));
        Saldo += valor;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Debitar(decimal valor)
    {
        if (valor <= 0) throw new ArgumentException("Valor a ser debitado deve ser maior que zero.", nameof(valor));
        if (valor > Saldo) throw new ArgumentException("Saldo insuficiente para esta operação.");
        Saldo -= valor;
        DataAtualizacao = DateTime.UtcNow;
    }
}