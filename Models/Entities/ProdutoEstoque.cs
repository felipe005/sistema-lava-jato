using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaLavaJato.Web.Models.Entities;

public class ProdutoEstoque
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(60)]
    public string? Unidade { get; set; }

    public int QuantidadeAtual { get; set; }

    public int QuantidadeMinima { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal CustoUnitario { get; set; }

    public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = new List<MovimentacaoEstoque>();
}
