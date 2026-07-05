using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using _2_Payment.Application.Dtos;
using _2_Payment.Application.Interfaces;

namespace _1_Payment.Api.Controllers
{
    /// <summary>
    /// API para operações de conta e saldo.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContaController : ControllerBase
    {
        private readonly IContaService _contaService;

        public ContaController(IContaService contaService)
        {
            _contaService = contaService;
        }

        /// <summary>
        /// Obtém o saldo de uma conta.
        /// </summary>
        /// <param name="idConta">Identificador da conta.</param>
        [HttpGet("{idConta}/saldo")]
        public async Task<IActionResult> ObterSaldo(int idConta)
        {
            var saldo = await _contaService.ObterSaldo(idConta);
            return Ok(saldo);
        }

        /// <summary>
        /// Adiciona saldo a uma conta.
        /// </summary>
        /// <param name="dto">Dados da conta e valor a adicionar.</param>
        [Authorize(Roles = "Admin")]
        [HttpPost("adicionar-saldo")]
        public async Task<IActionResult> AdicionarSaldo([FromBody] ContaDto dto)
        {
            var conta = await _contaService.AdicionarSaldo(dto);
            return Ok(conta);
        }
    }
}
