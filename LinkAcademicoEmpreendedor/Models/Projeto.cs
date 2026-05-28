using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Projeto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A Descrição é obrigatória")]
        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Tipo { get; set; }

        [StringLength(200)]
        public string? Tecnologias { get; set; }

        [StringLength(300)]
        public string? LinkRepositorio { get; set; }

        [StringLength(300)]
        public string? LinkDemonstracao { get; set; }

        public string? ImagemCapa { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public bool Ativo { get; set; } = true;

        // CHAVE ESTRANGEIRA
        public int AlunoId { get; set; }

        [ForeignKey("AlunoId")]
        public virtual Aluno? Aluno { get; set; }

        // NAVEGAÇÃO
        public virtual ICollection<Curtida> Curtidas { get; set; } = new List<Curtida>();
        public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public virtual ICollection<ProjetoLink> Links { get; set; } = new List<ProjetoLink>();

        [NotMapped]
        public int TotalCurtidas => Curtidas?.Count ?? 0;

        [NotMapped]
        public int TotalComentarios => Comentarios?.Count ?? 0;

        [StringLength(100)]
        public string? Area { get; set; }

        // ============================
        // JSON DOS CAMPOS DINÂMICOS
        // ============================
        public string? DadosDinamicosJson { get; set; }

        [NotMapped]
        public Dictionary<string, string> DadosDinamicos
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DadosDinamicosJson))
                    return new Dictionary<string, string>();

                try
                {
                    return JsonSerializer.Deserialize<Dictionary<string, string>>(DadosDinamicosJson)
                           ?? new Dictionary<string, string>();
                }
                catch
                {
                    return new Dictionary<string, string>();
                }
            }
            set
            {
                DadosDinamicosJson = JsonSerializer.Serialize(value);
            }
        }

        public string? ArquivoPdf { get; set; }
    }
}