using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Projeto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;

        // Compatível com código existente: mantém string Tipo (ex: Pesquisa, Extensão, TCC, Pessoal...)
        [StringLength(100)]
        public string? Tipo { get; set; }

        // Nova propriedade: Curso / Área do projeto (ex: Engenharia, Ciência da Computação, Gestão)
        [StringLength(200)]
        public string? Area { get; set; }

        [StringLength(200)]
        public string? Tecnologias { get; set; }

        // Link específico para repositório (mantém compatibilidade)
        [StringLength(300)]
        public string? LinkRepositorio { get; set; }

        // Link de demonstração ou hospedagem (mantém compatibilidade)
        [StringLength(300)]
        public string? LinkDemonstracao { get; set; }

        // Novo: Link Universal do projeto (canonical / landing page)
        [StringLength(500)]
        public string? LinkUniversal { get; set; }

        public string? ImagemCapa { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public bool Ativo { get; set; } = true;

        // Chave estrangeira
        public int AlunoId { get; set; }

        [ForeignKey("AlunoId")]
        public virtual Aluno? Aluno { get; set; }

        // Navegação
        public virtual ICollection<Curtida> Curtidas { get; set; } = new List<Curtida>();
        public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();

        // Estrutura escalável para links/recursos relacionados ao projeto (GitHub, Demo, DOI, Artigo, Slides, Landing Page, etc.)
        public virtual ICollection<ProjetoLink> Links { get; set; } = new List<ProjetoLink>();

        [NotMapped]
        public int TotalCurtidas => Curtidas?.Count ?? 0;

        [NotMapped]
        public int TotalComentarios => Comentarios?.Count ?? 0;
    }
}