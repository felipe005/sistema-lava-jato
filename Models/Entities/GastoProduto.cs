using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaLavaJato.Web.Models.Entities;

public class GastoProduto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Produto { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal ValorUnitario { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; } = 1;

    public DateTime Data { get; set; } = DateTime.UtcNow;

    [MaxLength(300)]
    public string? Observacoes { get; set; }

    [NotMapped]
    public decimal Total => ValorUnitario * Quantidade;
}
