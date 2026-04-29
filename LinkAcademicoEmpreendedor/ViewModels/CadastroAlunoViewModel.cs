using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class CadastroAlunoViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome Completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha")]
        [Compare("Senha", ErrorMessage = "As senhas não conferem")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        public string ConfirmarSenha { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Curso { get; set; }

        [StringLength(100)]
        [Display(Name = "Instituição")]
        public string? Instituicao { get; set; }

        [Display(Name = "Ano de Ingresso")]
        public int? AnoIngresso { get; set; }
    }
}