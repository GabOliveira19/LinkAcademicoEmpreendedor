using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Curtida
    {
        public int Id { get; set; }

        public DateTime DataCurtida { get; set; } = DateTime.Now;

        // Chaves estrangeiras
        public int AlunoId { get; set; }
        public int ProjetoId { get; set; }

        [ForeignKey("AlunoId")]
        public virtual Aluno? Aluno { get; set; }

        [ForeignKey("ProjetoId")]
        public virtual Projeto? Projeto { get; set; }
    }
}