using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Comentario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O texto é obrigatório")]
        [StringLength(500)]
        public string Texto { get; set; } = string.Empty;

        public DateTime DataComentario { get; set; } = DateTime.Now;

        // Chaves estrangeiras
        public int AlunoId { get; set; }
        public int ProjetoId { get; set; }

        [ForeignKey("AlunoId")]
        public virtual Aluno? Aluno { get; set; }

        [ForeignKey("ProjetoId")]
        public virtual Projeto? Projeto { get; set; }
    }
}