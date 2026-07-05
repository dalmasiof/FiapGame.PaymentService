using Application.Interfaces;
using Entities;
using Enum;
using IntegrationEvents;
using System.Security.Cryptography;
using System.Text;

namespace Service;

public class PagamentoService : IPagamentoService
{
    private readonly IPagamentoRepository _pagamentoRepository;
    private readonly ICompraRepository _compraRepository;
    private readonly IContaRepository _contaRepository;
    private readonly IMessagePublisher _publisher;

    public PagamentoService(
        IPagamentoRepository pagamentoRepository,
        ICompraRepository compraRepository,
        IContaRepository contaRepository,
        IMessagePublisher publisher)
    {
        _pagamentoRepository = pagamentoRepository;
        _compraRepository = compraRepository;
        _contaRepository = contaRepository;
        _publisher = publisher;
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

    public async Task ProcessarCompraSolicitadaAsync(CompraSolicitadaIntegrationEvent evento)
    {
        var compraProcessada = await _compraRepository.ObterPorCompraCatalogoId(evento.CompraId);

        if (compraProcessada is not null)
        {
            var pagamentos = await _pagamentoRepository.ObterPorUsuario(evento.UsuarioId);
            var pagamentoExistente = pagamentos.FirstOrDefault(p => p.IdCompra == compraProcessada.Id);

            if (pagamentoExistente is not null)
            {
                var jaAprovado = pagamentoExistente.Status == STATUS_PAGAMENTO.CONCLUIDO;

                if (jaAprovado)
                {
                    await PublicarNotificacaoPagamentoAprovadoAsync(evento);
                }

                await PublicarResultadoAsync(
                    evento,
                    jaAprovado,
                    pagamentoExistente.Status,
                    jaAprovado ? null : "Compra já processada anteriormente.");
                return;
            }
        }

        var compra = new Compra(0, evento.UsuarioId, evento.CompraId, evento.ValorTotal);

        foreach (var jogoId in evento.JogosIds.Distinct())
        {
            compra.AdicionarItem(jogoId, 0m, 0m);
        }

        await _compraRepository.Add(compra);

        var pagamento = new Pagamento(0, compra.Id);
        var aprovado = false;
        string? motivoRecusa = null;

        var conta = await _contaRepository.ObterContaPorUsuarioId(evento.UsuarioId);

        if (conta is null)
        {
            motivoRecusa = "Conta do usuário não encontrada.";
            pagamento.RecusarPagamento();
        }
        else if (conta.Saldo < evento.ValorTotal)
        {
            motivoRecusa = "Saldo insuficiente para esta operação.";
            pagamento.RecusarPagamento();
        }
        else
        {
            conta.Debitar(evento.ValorTotal);
            await _contaRepository.DebitarSaldo(conta, evento.ValorTotal);
            pagamento.ConfirmarPagamento();
            aprovado = true;
        }

        await _pagamentoRepository.Add(pagamento);

        if (aprovado)
        {
            await PublicarNotificacaoPagamentoAprovadoAsync(evento);
        }

        await PublicarResultadoAsync(evento, aprovado, pagamento.Status, motivoRecusa);
    }

    private Task PublicarNotificacaoPagamentoAprovadoAsync(CompraSolicitadaIntegrationEvent evento)
    {
        var destinatario = ObterDestinatarioNotificacao(evento);

        if (string.IsNullOrWhiteSpace(destinatario))
        {
            return Task.CompletedTask;
        }

        var notificacao = new NotificacaoIntegrationEvent(
            CorrelacaoId: CriarCorrelacaoId(evento.CompraId),
            Destinatario: destinatario,
            Assunto: "Pagamento Aprovado",
            CorpoMensagem: $"Olá! Seu pagamento da compra {evento.CompraId} no valor de {evento.ValorTotal:C} foi aprovado.",
            DominioOrigem: "Pagamento")
        {
            RastreioId = evento.RastreioId
        };

        return _publisher.PublishAsync(
            notificacao,
            exchange: "notificacao.exchange",
            routingKey: "pagamento.notificacao",
            correlationId: evento.RastreioId);
    }

    private static string? ObterDestinatarioNotificacao(CompraSolicitadaIntegrationEvent evento)
    {
        return !string.IsNullOrWhiteSpace(evento.EmailUsuario)
            ? evento.EmailUsuario
            : evento.DestinatarioNotificacao;
    }

    private Task PublicarResultadoAsync(
        CompraSolicitadaIntegrationEvent evento,
        bool aprovado,
        STATUS_PAGAMENTO status,
        string? motivoRecusa)
    {
        var resultado = new PagamentoProcessadoIntegrationEvent(
            CompraId: evento.CompraId,
            UsuarioId: evento.UsuarioId,
            Aprovado: aprovado,
            ValorTotal: evento.ValorTotal,
            Status: status.ToString(),
            ProcessadoEm: DateTime.UtcNow,
            RastreioId: evento.RastreioId,
            MotivoRecusa: motivoRecusa);

        var routingKey = aprovado ? "pagamento.aprovado" : "pagamento.recusado";
        return _publisher.PublishAsync(resultado, routingKey, evento.RastreioId);
    }

    private static Guid CriarCorrelacaoId(int compraId)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes($"Pagamento:{compraId}"));
        return new Guid(bytes);
    }
}
