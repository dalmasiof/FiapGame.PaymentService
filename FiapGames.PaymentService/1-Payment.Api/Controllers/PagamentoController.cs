using Microsoft.AspNetCore.Mvc;
using _2_Payment.Application.Interfaces;

namespace _1_Payment.Api.Controllers
{
    /// <summary>
    /// API de pagamentos do FIAP Games.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PagamentoController : ControllerBase
    {
        private readonly IPagamentoService _pagamentoService;

        public PagamentoController(IPagamentoService pagamentoService)
        {
            _pagamentoService = pagamentoService;
        }

        /// <summary>
        /// Obtém o histórico de pagamentos de um usuário.
        /// </summary>
        /// <param name="idUsuario">Identificador do usuário.</param>
        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObterPorUsuario(int idUsuario)
        {
            var historico = await _pagamentoService.ObterPorUsuario(idUsuario);
            return Ok(historico);
        }
    }
}
