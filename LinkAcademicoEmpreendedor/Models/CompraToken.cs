using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkAcademicoEmpreendedor.Models
{
    public class CompraToken
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int PacoteTokenId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }

        public int QuantidadeTokens { get; set; }

        public StatusPagamentoCompraToken StatusPagamento { get; set; } = StatusPagamentoCompraToken.Pendente;

        [StringLength(100)]
        public string? MercadoPagoPaymentId { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime? DataPagamento { get; set; }

        [ForeignKey("UsuarioId")]
        public Aluno? Usuario { get; set; }

        [ForeignKey("PacoteTokenId")]
        public PacoteToken? PacoteToken { get; set; }
    }
}