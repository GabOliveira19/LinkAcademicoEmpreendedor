using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.Services;
using MercadoPago.Resource.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LinkAcademicoEmpreendedor.Controllers
{
    [ApiController]
    [Route("api/webhooks/mercadopago")]
    public class MercadoPagoWebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MercadoPagoService _mercadoPagoService;
        private readonly ILogger<MercadoPagoWebhookController> _logger;

        public MercadoPagoWebhookController(ApplicationDbContext context ,MercadoPagoService mercadoPagoService, ILogger<MercadoPagoWebhookController> logger)
        {
            _context = context;
            _mercadoPagoService = mercadoPagoService;
            _logger = logger;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ReceberNotificacao([FromQuery] string? type, [FromQuery] string? topic, [FromQuery] long? id, [FromBody] JsonElement body)
        {
            _logger.LogInformation("TYPE: {Type}", type);
            _logger.LogInformation("TOPIC: {Topic}", topic);
            _logger.LogInformation("BODY: {Body}", body.ToString());

            var paymentId = id ?? ObterPaymentId(body);

            if (paymentId == null)
            {
                _logger.LogWarning("Webhook sem paymentId");
                return Ok();
            }

            var notificationType = type ?? topic;

            if (!string.Equals(notificationType, "payment", StringComparison.OrdinalIgnoreCase))
                return Ok();

            MercadoPago.Resource.Payment.Payment pagamento;

            try
            {
                pagamento = await _mercadoPagoService.ObterPagamentoAsync(paymentId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar pagamento");
                return Ok();
            }

            var externalReference = pagamento.ExternalReference;

            if (string.IsNullOrWhiteSpace(externalReference))
                return Ok();

            // ============ ROUTER DE PAGAMENTOS
      

            if (externalReference.StartsWith("COMPRA_TOKEN_"))
            {
                return await ProcessarCompraToken(pagamento, paymentId.Value, externalReference);
            }

            if (externalReference.StartsWith("PREMIUM_"))
            {
                return await ProcessarAssinaturaPremium(pagamento, paymentId.Value, externalReference);
            }

            _logger.LogWarning("ExternalReference desconhecida: {Ref}", externalReference);
            return Ok();
        }

        private async Task<IActionResult> ProcessarCompraToken(Payment pagamento, long paymentId, string externalReference)
        {
            if (!int.TryParse(externalReference.Replace("COMPRA_TOKEN_", ""), out var compraId))
                return Ok();

            var compra = await _context.ComprasToken
                .FirstOrDefaultAsync(c => c.Id == compraId);

            if (compra == null || compra.StatusPagamento == StatusPagamentoCompraToken.Aprovado)
                return Ok();

            compra.MercadoPagoPaymentId = paymentId.ToString();

            var status = pagamento.Status?.ToLowerInvariant();

            if (status == "approved")
            {
                compra.StatusPagamento = StatusPagamentoCompraToken.Aprovado;
                compra.DataPagamento = DateTime.Now;

                var carteira = await _context.CarteirasToken
                    .FirstOrDefaultAsync(c => c.AlunoId == compra.UsuarioId);

                if (carteira == null)
                {
                    carteira = new CarteiraToken
                    {
                        AlunoId = compra.UsuarioId,
                        Saldo = 0
                    };
                    _context.CarteirasToken.Add(carteira);
                }

                carteira.Saldo += compra.QuantidadeTokens;

                _context.MovimentacoesToken.Add(new MovimentacaoToken
                {
                    AlunoId = compra.UsuarioId,
                    Quantidade = compra.QuantidadeTokens,
                    Tipo = $"Compra de tokens ({compra.PacoteTokenId})",
                    Data = DateTime.Now
                });
            }
            else if (status == "rejected")
            {
                compra.StatusPagamento = StatusPagamentoCompraToken.Recusado;
            }
            else if (status == "cancelled" || status == "refunded" || status == "charged_back")
            {
                compra.StatusPagamento = StatusPagamentoCompraToken.Cancelado;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<IActionResult> ProcessarAssinaturaPremium(Payment pagamento, long paymentId, string externalReference)
        {
            if (!int.TryParse(externalReference.Replace("PREMIUM_", ""), out var assinaturaId))
                return Ok();

            var assinatura = await _context.AssinaturasPremium.FirstOrDefaultAsync(x => x.Id == assinaturaId);

            if (assinatura == null || assinatura.Status == "Ativo")
                return Ok();

            assinatura.MercadoPagoPaymentId = paymentId.ToString();

            var status = pagamento.Status?.ToLowerInvariant();

            if (status == "approved")
            {
                assinatura.Status = "Ativo";
                assinatura.Inicio = DateTime.Now;
                assinatura.Fim = DateTime.Now.AddMonths(1);
            }
            else if (status == "rejected")
            {
                assinatura.Status = "Recusado";
            }
            else if (status == "cancelled")
            {
                assinatura.Status = "Cancelado";
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private static long? ObterPaymentId(JsonElement body)
        {
            try
            {
                // 1. Tenta data.id
                if (body.TryGetProperty("data", out var data) &&
                    data.ValueKind == JsonValueKind.Object &&
                    data.TryGetProperty("id", out var idElement) &&
                    idElement.TryGetInt64(out var id))
                {
                    return id;
                }

                // 2. Tenta id direto no root
                if (body.TryGetProperty("id", out var rootId) &&
                    rootId.TryGetInt64(out var id2))
                {
                    return id2;
                }

                // 3. Tenta se veio como string 
                if (body.TryGetProperty("data", out var d) &&
                    d.TryGetProperty("id", out var idStrElement) &&
                    idStrElement.ValueKind == JsonValueKind.String &&
                    long.TryParse(idStrElement.GetString(), out var idStr))
                {
                    return idStr;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}