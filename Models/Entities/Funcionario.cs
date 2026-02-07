using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaLavaJato.Web.Models.Entities;

public class Funcionario
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal SalarioMensal { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal PercentualComissao { get; set; }

    public bool Ativo { get; set; } = true;

    [MaxLength(450)]
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
