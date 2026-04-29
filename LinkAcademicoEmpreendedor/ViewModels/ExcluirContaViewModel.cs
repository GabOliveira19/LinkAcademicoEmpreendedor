using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.ViewModels
{
    public class ExcluirContaViewModel
    {
        [Required(ErrorMessage = "A senha e obrigatoria para confirmar a exclusao")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Digite 'EXCLUIR' para confirmar")]
        public string ConfirmacaoTexto { get; set; } = string.Empty;
    }
}