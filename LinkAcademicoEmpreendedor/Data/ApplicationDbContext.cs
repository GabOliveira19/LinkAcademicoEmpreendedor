using Microsoft.EntityFrameworkCore;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Projeto> Projetos { get; set; }
        public DbSet<ProjetoLink> ProjetoLinks { get; set; }
        public DbSet<Oportunidade> Oportunidades { get; set; }
        public DbSet<Curtida> Curtidas { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Candidatura> Candidaturas { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Area> Areas { get; set; }

        public DbSet<FieldDefinition> FieldDefinitions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Aluno>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<Empresa>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Empresa>()
                .HasIndex(e => e.Cnpj)
                .IsUnique();

            modelBuilder.Entity<Curtida>()
                .HasIndex(c => new { c.AlunoId, c.EmpresaId, c.ProjetoId })
                .IsUnique();

            modelBuilder.Entity<Candidatura>()
                .HasIndex(c => new { c.AlunoId, c.OportunidadeId })
                .IsUnique();

            modelBuilder.Entity<Projeto>()
                .HasOne(p => p.Aluno)
                .WithMany(a => a.Projetos)
                .HasForeignKey(p => p.AlunoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjetoLink>()
                .HasOne(pl => pl.Projeto)
                .WithMany(p => p.Links)
                .HasForeignKey(pl => pl.ProjetoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Oportunidade>()
                .HasOne(o => o.Empresa)
                .WithMany(e => e.Oportunidades)
                .HasForeignKey(o => o.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Curtida>()
                .HasOne(c => c.Aluno)
                .WithMany(a => a.Curtidas)
                .HasForeignKey(c => c.AlunoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Curtida>()
                .HasOne(c => c.Empresa)
                .WithMany(e => e.Curtidas)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Curtida>()
                .HasOne(c => c.Projeto)
                .WithMany(p => p.Curtidas)
                .HasForeignKey(c => c.ProjetoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Aluno)
                .WithMany(a => a.Comentarios)
                .HasForeignKey(c => c.AlunoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Empresa)
                .WithMany(e => e.Comentarios)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Comentario>()
                .HasOne(c => c.Projeto)
                .WithMany(p => p.Comentarios)
                .HasForeignKey(c => c.ProjetoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Candidatura>()
                .HasOne(c => c.Aluno)
                .WithMany(a => a.Candidaturas)
                .HasForeignKey(c => c.AlunoId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Candidatura>()
                .HasOne(c => c.Oportunidade)
                .WithMany(o => o.Candidaturas)
                .HasForeignKey(c => c.OportunidadeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Aluno>()
                .HasOne(a => a.Area)
                .WithMany()
                .HasForeignKey(a => a.AreaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FieldDefinition>()
                .HasOne(f => f.Area)
                .WithMany()
                .HasForeignKey(f => f.AreaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Area>().HasData(
                new Area { Id = 1, Nome = "Interdisciplinar", Descricao = "Área padrão / multiáreas" },
                new Area { Id = 2, Nome = "Tecnologia", Descricao = "Ciências da Computação, Sistemas, TI" },
                new Area { Id = 3, Nome = "Saúde", Descricao = "Medicina, Enfermagem, Farmácia" },
                new Area { Id = 4, Nome = "Engenharia", Descricao = "Engenharias diversas" },
                new Area { Id = 5, Nome = "Direito", Descricao = "Ciências Jurídicas" },
                new Area { Id = 6, Nome = "Negócios", Descricao = "Administração, Economia" },
                new Area { Id = 7, Nome = "Design/Artes", Descricao = "Design, Artes Visuais, Multimídia" },
                new Area { Id = 8, Nome = "Educação", Descricao = "Pedagogia, Educação" },
                new Area { Id = 9, Nome = "Ciências Sociais", Descricao = "Sociologia, Psicologia, Antropologia" }
            );

            modelBuilder.Entity<FieldDefinition>().HasData(
                new FieldDefinition { Id = 1, AreaId = 2, Nome = "GitHub", Tipo = "url", Obrigatorio = false, Ordem = 1 },
                new FieldDefinition { Id = 2, AreaId = 2, Nome = "StackUtilizada", Tipo = "string", Obrigatorio = true, Ordem = 2 },
                new FieldDefinition { Id = 3, AreaId = 2, Nome = "Tecnologias", Tipo = "string", Obrigatorio = false, Ordem = 3 },
                new FieldDefinition { Id = 4, AreaId = 2, Nome = "Deploy", Tipo = "string", Obrigatorio = false, Ordem = 4 },
                new FieldDefinition { Id = 5, AreaId = 2, Nome = "DocumentacaoTecnica", Tipo = "textarea", Obrigatorio = false, Ordem = 5 },

                new FieldDefinition { Id = 10, AreaId = 3, Nome = "AreaPesquisaClinica", Tipo = "string", Obrigatorio = true, Ordem = 1 },
                new FieldDefinition { Id = 11, AreaId = 3, Nome = "InstituicaoAplicacao", Tipo = "string", Obrigatorio = true, Ordem = 2 },
                new FieldDefinition { Id = 12, AreaId = 3, Nome = "MetodologiaCientifica", Tipo = "textarea", Obrigatorio = true, Ordem = 3 },
                new FieldDefinition { Id = 13, AreaId = 3, Nome = "ArtigosReferencias", Tipo = "textarea", Obrigatorio = false, Ordem = 4 },
                new FieldDefinition { Id = 14, AreaId = 3, Nome = "AprovacaoEtica", Tipo = "boolean", Obrigatorio = false, Ordem = 5 },
                new FieldDefinition { Id = 15, AreaId = 3, Nome = "DocumentoCientifico", Tipo = "file", Obrigatorio = false, Ordem = 6 },

                new FieldDefinition { Id = 20, AreaId = 4, Nome = "MateriaisUtilizados", Tipo = "textarea", Obrigatorio = false, Ordem = 1 },
                new FieldDefinition { Id = 21, AreaId = 4, Nome = "CalculosNormas", Tipo = "textarea", Obrigatorio = true, Ordem = 2 },
                new FieldDefinition { Id = 22, AreaId = 4, Nome = "SoftwaresUtilizados", Tipo = "string", Obrigatorio = false, Ordem = 3 },
                new FieldDefinition { Id = 23, AreaId = 4, Nome = "ModelosProjetos", Tipo = "file", Obrigatorio = false, Ordem = 4 },

                new FieldDefinition { Id = 30, AreaId = 7, Nome = "FerramentasUtilizadas", Tipo = "string", Obrigatorio = false, Ordem = 1 },
                new FieldDefinition { Id = 31, AreaId = 7, Nome = "BehanceFigma", Tipo = "url", Obrigatorio = false, Ordem = 2 },
                new FieldDefinition { Id = 32, AreaId = 7, Nome = "Prototipo", Tipo = "url", Obrigatorio = false, Ordem = 3 },

                new FieldDefinition { Id = 40, AreaId = 5, Nome = "AreaJuridica", Tipo = "string", Obrigatorio = true, Ordem = 1 },
                new FieldDefinition { Id = 41, AreaId = 5, Nome = "CasoEstudo", Tipo = "textarea", Obrigatorio = false, Ordem = 2 },
                new FieldDefinition { Id = 42, AreaId = 5, Nome = "DocumentoDrive", Tipo = "url", Obrigatorio = false, Ordem = 3 },

                new FieldDefinition { Id = 50, AreaId = 6, Nome = "MercadoAlvo", Tipo = "string", Obrigatorio = false, Ordem = 1 },
                new FieldDefinition { Id = 51, AreaId = 6, Nome = "PlanoNegocios", Tipo = "textarea", Obrigatorio = false, Ordem = 2 },

                new FieldDefinition { Id = 60, AreaId = 9, Nome = "Metodologia", Tipo = "textarea", Obrigatorio = false, Ordem = 1 },
                new FieldDefinition { Id = 61, AreaId = 9, Nome = "Referencias", Tipo = "textarea", Obrigatorio = false, Ordem = 2 }
            );
        }
    }
}