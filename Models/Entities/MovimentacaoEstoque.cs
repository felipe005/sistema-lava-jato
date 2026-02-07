using System.ComponentModel.DataAnnotations;

namespace SistemaLavaJato.Web.Models.Entities;

public class MovimentacaoEstoque
{
    public int Id { get; set; }

    [Required]
    public int ProdutoEstoqueId { get; set; }
    public ProdutoEstoque? ProdutoEstoque { get; set; }

    public TipoMovimentacaoEstoque Tipo { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? Observacoes { get; set; }
}
