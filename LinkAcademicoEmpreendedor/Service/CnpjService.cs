using System.Text.Json;

namespace LinkAcademicoEmpreendedor.Services
{
    public class CnpjService
    {
        private readonly HttpClient _httpClient;

        public CnpjService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<CnpjResponse?> ConsultarCnpjAsync(string cnpj)
        {
            try
            {
                // Remove caracteres especiais do CNPJ
                string cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());

                if (cnpjLimpo.Length != 14)
                    return null;

                // Usando BrasilAPI (gratuita)
                var url = $"https://brasilapi.com.br/api/cnpj/v1/{cnpjLimpo}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var dados = JsonSerializer.Deserialize<BrasilApiCnpjResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dados == null)
                    return null;

                return new CnpjResponse
                {
                    Cnpj = dados.Cnpj ?? cnpjLimpo,
                    RazaoSocial = dados.Razao_social ?? "",
                    NomeFantasia = dados.Nome_fantasia ?? dados.Razao_social ?? "",
                    SituacaoCadastral = dados.Descricao_situacao_cadastral ?? "",
                    DataAbertura = dados.Data_inicio_atividade ?? "",
                    NaturezaJuridica = dados.Natureza_juridica ?? "",
                    Endereco = dados.Logradouro ?? "",
                    Numero = dados.Numero ?? "",
                    Bairro = dados.Bairro ?? "",
                    Cidade = dados.Municipio ?? "",
                    Estado = dados.Uf ?? "",
                    Cep = dados.Cep ?? "",
                    Sucesso = true
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool ValidarCnpj(string cnpj)
        {
            string cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());

            if (cnpjLimpo.Length != 14)
                return false;

            // Verifica se todos os digitos sao iguais
            if (cnpjLimpo.Distinct().Count() == 1)
                return false;

            // Calcula primeiro digito verificador
            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpjLimpo.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            string digito = resto.ToString();
            tempCnpj += digito;

            // Calcula segundo digito verificador
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            digito += resto.ToString();

            return cnpjLimpo.EndsWith(digito);
        }
    }

    public class CnpjResponse
    {
        public string Cnpj { get; set; } = string.Empty;
        public string RazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; }
        public string? SituacaoCadastral { get; set; }
        public string? DataAbertura { get; set; }
        public string? NaturezaJuridica { get; set; }
        public string? Endereco { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Cep { get; set; }
        public bool Sucesso { get; set; }
        public string? Mensagem { get; set; }
    }

    // Classe para mapear resposta da BrasilAPI
    public class BrasilApiCnpjResponse
    {
        public string? Cnpj { get; set; }
        public string? Razao_social { get; set; }
        public string? Nome_fantasia { get; set; }
        public string? Descricao_situacao_cadastral { get; set; }
        public string? Data_inicio_atividade { get; set; }
        public string? Natureza_juridica { get; set; }
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Municipio { get; set; }
        public string? Uf { get; set; }
        public string? Cep { get; set; }
    }
}