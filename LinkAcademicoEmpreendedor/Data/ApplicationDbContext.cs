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
        public DbSet<Oportunidade> Oportunidades { get; set; }
        public DbSet<Curtida> Curtidas { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }
        public DbSet<Candidatura> Candidaturas { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        

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
                .HasIndex(c => new { c.AlunoId, c.ProjetoId })
                .IsUnique();

            // Candidatura unica por aluno/oportunidade
            modelBuilder.Entity<Candidatura>()
                .HasIndex(c => new { c.AlunoId, c.OportunidadeId })
                .IsUnique();

            modelBuilder.Entity<Projeto>()
                .HasOne(p => p.Aluno)
                .WithMany(a => a.Projetos)
                .HasForeignKey(p => p.AlunoId)
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
        }
    }
}