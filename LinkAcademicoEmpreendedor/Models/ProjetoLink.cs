using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    // Entidade para permitir múltiplos links por projeto de forma escalável
    public class ProjetoLink
    {
        public int Id { get; set; }

        // Tipo do link (ex: "Repositório", "Demostração", "Artigo", "Slides", "Landing", "DOI")
        [StringLength(100)]
        public string? Tipo { get; set; }

        // URL do recurso
        [StringLength(1000)]
        public string Url { get; set; } = string.Empty;

        // Relação com Projeto
        public int ProjetoId { get; set; }

        [ForeignKey("ProjetoId")]
        public virtual Projeto? Projeto { get; set; }
    }
}