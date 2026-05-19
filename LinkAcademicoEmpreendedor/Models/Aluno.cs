using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LinkAcademicoEmpreendedor.Models
{
    public class Aluno
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome e obrigatorio")]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email e obrigatorio")]
        [EmailAddress(ErrorMessage = "Email invalido")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha e obrigatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no minimo 6 caracteres")]
        public string Senha { get; set; } = string.Empty;

        // legacy: pode manter string Curso, mas iremos exigir AreaId
        [StringLength(200)]
        public string? Curso { get; set; }

        public int? AnoIngresso { get; set; }

        [StringLength(200)]
        public string? Instituicao { get; set; }

        [StringLength(2000)]
        public string? Sobre { get; set; }

        [StringLength(1000)]
        public string? Habilidades { get; set; }

        [StringLength(500)]
        public string? FotoPerfil { get; set; }

        [StringLength(300)]
        public string? LinkedIn { get; set; }

        [StringLength(300)]
        public string? GitHub { get; set; }



        public DateTime DataCadastro { get; set; } = DateTime.Now;

        // Campos para recuperacao de senha
        [StringLength(100)]
        public string? TokenRecuperacao { get; set; }

        public DateTime? TokenExpiracao { get; set; }

        // Relação com Area (nova)
        public int AreaId { get; set; }
        public Area? Area { get; set; }

        // Navegacao
        public virtual ICollection<Projeto> Projetos { get; set; } = new List<Projeto>();
        public virtual ICollection<Curtida> Curtidas { get; set; } = new List<Curtida>();
        public virtual ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public virtual ICollection<Candidatura> Candidaturas { get; set; } = new List<Candidatura>();

        public string? Curriculo { get; set; }

        public List<RedeSocial> RedesSociais { get; set; } = new List<RedeSocial>();
    }
}