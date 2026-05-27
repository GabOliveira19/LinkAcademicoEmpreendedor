using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class CadastroAlunoViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
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
        public string ConfirmarSenha { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Curso { get; set; }

        [StringLength(100)]
        public string? Instituicao { get; set; }

        public int? AnoIngresso { get; set; }

        // Nova propriedade obrigatória: AreaId
        [Required(ErrorMessage = "Selecione a área/campo principal")]
        public int AreaId { get; set; }
        public List<string>? Plataformas { get; set; }
        public List<string>? Urls { get; set; }
    }
}