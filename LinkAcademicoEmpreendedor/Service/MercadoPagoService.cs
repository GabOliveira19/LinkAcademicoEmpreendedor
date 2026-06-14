using LinkAcademicoEmpreendedor.Models;
using LinkAcademicoEmpreendedor.Settings;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Options;
using MercadoPago.Client.Common;

namespace LinkAcademicoEmpreendedor.Services
{
    public class MercadoPagoService
    {
        private readonly MercadoPagoSettings _settings;

        public MercadoPagoService(IOptions<MercadoPagoSettings> options)
        {
            _settings = options.Value;
        }

        public string AccessToken => _settings.AccessToken;

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.AccessToken);

        public void EnsureConfigured()
        {
            if (!IsConfigured)
                throw new InvalidOperationException("Mercado Pago nao configurado. Informe o AccessToken.");
        }

        public void ConfigureSdk()
        {
            EnsureConfigured();
            MercadoPagoConfig.AccessToken = _settings.AccessToken;
        }

        public bool TestConfiguration()
        {
            ConfigureSdk();
            return MercadoPagoConfig.AccessToken == _settings.AccessToken;
        }

        public async Task<Preference> CriarPreferenciaPacoteAsync(
            PacoteToken pacote,
            string successUrl,
            string pendingUrl,
            string failureUrl,
            string externalReference)
        {
            ConfigureSdk();

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = pacote.Nome,
                        Quantity = 1,
                        CurrencyId = "BRL",
                        UnitPrice = pacote.Valor
                    }
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = successUrl,
                    Pending = pendingUrl,
                    Failure = failureUrl
                },
                AutoReturn = "approved",
                ExternalReference = externalReference,
                NotificationUrl = "https://wimp-amaretto-apache.ngrok-free.dev/api/webhooks/mercadopago"
            };

            var client = new PreferenceClient();
            return await client.CreateAsync(request);
        }

        public async Task<Payment> ObterPagamentoAsync(long paymentId)
        {
            ConfigureSdk();
            var client = new PaymentClient();
            return await client.GetAsync(paymentId);
        }
        public async Task<Preference> CriarPreferenciaPlanoAsync(PlanoPremium plano,string successUrl,string pendingUrl,string failureUrl,string externalReference)
        {
            ConfigureSdk();

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = $"Plano Premium {plano.Nome}",
                        Quantity = 1,
                        CurrencyId = "BRL",
                        UnitPrice = plano.ValorMensal
                    }
                },
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = successUrl,
                    Pending = pendingUrl,
                    Failure = failureUrl
                },
                AutoReturn = "approved",
                ExternalReference = externalReference,
                NotificationUrl = "https://wimp-amaretto-apache.ngrok-free.dev/api/webhooks/mercadopago"
            };

            var client = new PreferenceClient();
            return await client.CreateAsync(request);
        }

        public async Task<Payment> ProcessarPagamentoTransparenteAsync(
            LinkAcademicoEmpreendedor.ViewModels.PaymentRequestDto paymentDto,
            string externalReference)
        {
            ConfigureSdk();

            var request = new PaymentCreateRequest
            {
                TransactionAmount = paymentDto.transaction_amount,
                Token = string.IsNullOrEmpty(paymentDto.token) ? null : paymentDto.token,
                Description = paymentDto.description,
                Installments = paymentDto.installments,
                PaymentMethodId = paymentDto.payment_method_id,
                IssuerId = string.IsNullOrEmpty(paymentDto.issuer_id) ? null : paymentDto.issuer_id,
                Payer = new PaymentPayerRequest
                {
                    Email = paymentDto.payer.email,
                    Identification = new IdentificationRequest
                    {
                        Type = paymentDto.payer.identification?.type,
                        Number = paymentDto.payer.identification?.number
                    }
                },
                ExternalReference = externalReference,
                NotificationUrl = "https://wimp-amaretto-apache.ngrok-free.dev/api/webhooks/mercadopago"
            };

            var client = new PaymentClient();
            return await client.CreateAsync(request);
        }
    }
}