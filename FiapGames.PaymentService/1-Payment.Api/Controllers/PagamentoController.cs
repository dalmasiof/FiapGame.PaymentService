using Microsoft.AspNetCore.Mvc;
using _2_Payment.Application.Interfaces;

namespace _1_Payment.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagamentoController : ControllerBase
    {
        private readonly IPagamentoService _pagamentoService;

        public PagamentoController(IPagamentoService pagamentoService)
        {
            _pagamentoService = pagamentoService;
        }

        [HttpGet("usuario/{idUsuario}")]
        public async Task<IActionResult> ObterPorUsuario(int idUsuario)
        {
            var historico = await _pagamentoService.ObterPorUsuario(idUsuario);
            return Ok(historico);
        }
    }
}
