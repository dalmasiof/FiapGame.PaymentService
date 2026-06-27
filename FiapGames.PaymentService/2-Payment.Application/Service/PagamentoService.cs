using System.Collections.Generic;
using System.Threading.Tasks;
using _2_Payment.Application.Interfaces;
using Domain;

namespace _2_Payment.Application.Service
{
    public class PagamentoService : IPagamentoService
    {
        private readonly IPagamentoRepository _pagamentoRepository;

        public PagamentoService(IPagamentoRepository pagamentoRepository)
        {
            _pagamentoRepository = pagamentoRepository;
        }

        public Task Add(Pagamento pagamento)
        {
            return _pagamentoRepository.Add(pagamento);
        }

        public Task Update(Pagamento pagamento)
        {
            return _pagamentoRepository.Update(pagamento);
        }

        public Task<IEnumerable<Pagamento>> ObterPorUsuario(int idUsuario)
        {
            return _pagamentoRepository.ObterPorUsuario(idUsuario);
        }
    }
}
