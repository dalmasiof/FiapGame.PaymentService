using Microsoft.AspNetCore.Mvc;
using _2_Payment.Application.Dtos;
using _2_Payment.Application.Interfaces;

namespace _1_Payment.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContaController : ControllerBase
    {
        private readonly IContaService _contaService;

        public ContaController(IContaService contaService)
        {
            _contaService = contaService;
        }

        [HttpGet("{idConta}/saldo")]
        public async Task<IActionResult> ObterSaldo(int idConta)
        {
            var saldo = await _contaService.ObterSaldo(idConta);
            return Ok(saldo);
        }

        [HttpPost("adicionar-saldo")]
        public async Task<IActionResult> AdicionarSaldo([FromBody] ContaDto dto)
        {
            var conta = await _contaService.AdicionarSaldo(dto);
            return Ok(conta);
        }
    }
}
