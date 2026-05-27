using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class CadastroEmpresaViewModel
    {
        [Required(ErrorMessage = "O CNPJ e obrigatorio")]
        [StringLength(18)]
        [Display(Name = "CNPJ")]
        public string Cnpj { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Razao Social e obrigatoria")]
        [StringLength(200)]
        [Display(Name = "Razao Social")]
        public string RazaoSocial { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Nome Fantasia")]
        public string? NomeFantasia { get; set; }

        [StringLength(50)]
        [Display(Name = "Situacao Cadastral")]
        public string? SituacaoCadastral { get; set; }

        [Display(Name = "Data de Abertura")]
        public string? DataAbertura { get; set; }

        [StringLength(100)]
        [Display(Name = "Natureza Juridica")]
        public string? NaturezaJuridica { get; set; }

        [StringLength(200)]
        [Display(Name = "Endereco")]
        public string? Endereco { get; set; }

        [StringLength(20)]
        [Display(Name = "Numero")]
        public string? Numero { get; set; }

        [StringLength(100)]
        [Display(Name = "Bairro")]
        public string? Bairro { get; set; }

        [StringLength(100)]
        [Display(Name = "Cidade")]
        public string? Cidade { get; set; }

        [StringLength(2)]
        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        [StringLength(10)]
        [Display(Name = "CEP")]
        public string? Cep { get; set; }

        [Required(ErrorMessage = "O e-mail e obrigatorio")]
        [EmailAddress(ErrorMessage = "E-mail invalido")]
        [StringLength(100)]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha e obrigatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no minimo 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha")]
        [Compare("Senha", ErrorMessage = "As senhas nao conferem")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        public string ConfirmarSenha { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [StringLength(500)]
        [Display(Name = "Descricao da Empresa")]
        public string? Descricao { get; set; }

        [StringLength(100)]
        [Display(Name = "Area de Atuacao")]
        public string? AreaAtuacao { get; set; }

        [StringLength(300)]
        [Display(Name = "Logo da Empresa")]
        public string? LogoEmpresa { get; set; }

        [StringLength(100)]
        [Display(Name = "Nome do Responsavel")]
        public string? NomeResponsavel { get; set; }

        public List<string>? Plataformas { get; set; }
        public List<string>? Urls { get; set; }

        // Flag para indicar se os dados do CNPJ foram validados
        public bool CnpjValidado { get; set; } = false;
    }
}