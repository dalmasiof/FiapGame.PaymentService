namespace Domain
{
    public class Pagamento
    {
        public int IdPagamento { get; }
        public int IdCompra { get; }
        public virtual Compra? Compra { get; set; }
        public DateTime DataHoraInclusao { get; }
        public DateTime? DataHoraAlteracao { get; set; }
        public STATUS_PAGAMENTO Status { get; set; }

        public Pagamento()
        {
            
        }

        public Pagamento(int idPagamento, int idCompra)
        {
            IdPagamento = idPagamento;
            IdCompra = idCompra;
            DataHoraInclusao = DateTime.Now;
            Status = STATUS_PAGAMENTO.AGUARDANDO_PAGAMENTO;
        }

        public void RecusarPagamento()
        {
            DataHoraAlteracao = DateTime.Now;
            Status = STATUS_PAGAMENTO.RECUSADO;
        }

        public void ConfirmarPagamento()
        {
            DataHoraAlteracao = DateTime.Now;
            Status = STATUS_PAGAMENTO.CONCLUIDO;
        }


    }
}
