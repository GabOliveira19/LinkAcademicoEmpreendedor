using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class FavoritoVaga
    {
        public int Id { get; set; }

        public int AlunoId { get; set; }
        public int OportunidadeId { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        [ForeignKey("AlunoId")]
        public Aluno? Aluno { get; set; }

        [ForeignKey("OportunidadeId")]
        public Oportunidade? Oportunidade { get; set; }
    }
}
