using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class EditarPerfilEmpresaViewModel
    {
        public int Id { get; set; }

        [Display(Name = "CNPJ")]
        public string? Cnpj { get; set; }

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
        public DateTime? DataAbertura { get; set; }

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
    }
}