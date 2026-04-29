using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class EsqueceuSenhaViewModel
    {
        [Required(ErrorMessage = "O email e obrigatorio")]
        [EmailAddress(ErrorMessage = "Email invalido")]
        public string Email { get; set; } = string.Empty;

        public string TipoUsuario { get; set; } = "Aluno";
    }

    public class RedefinirSenhaViewModel
    {
        public string Token { get; set; } = string.Empty;

        public string TipoUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha e obrigatoria")]
        [MinLength(6, ErrorMessage = "A senha deve ter no minimo 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string NovaSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha")]
        [Compare("NovaSenha", ErrorMessage = "As senhas nao conferem")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Senha")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }

    public class AlterarSenhaViewModel
    {
        [Required(ErrorMessage = "A senha atual e obrigatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha Atual")]
        public string SenhaAtual { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha e obrigatoria")]
        [MinLength(6, ErrorMessage = "A senha deve ter no minimo 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string NovaSenha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme a senha")]
        [Compare("NovaSenha", ErrorMessage = "As senhas nao conferem")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Nova Senha")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }
}