using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaLavaJato.Web.Models.Entities;

public class Agendamento
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Required]
    public int VeiculoId { get; set; }
    public Veiculo? Veiculo { get; set; }

    [Required]
    public int ServicoId { get; set; }
    public Servico? Servico { get; set; }

    public int? FuncionarioId { get; set; }
    public Funcionario? Funcionario { get; set; }

    [Required]
    public DateTime DataAgendada { get; set; }

    public StatusServico Status { get; set; } = StatusServico.Pendente;

    public DateTime? InicioServico { get; set; }

    public int? TempoEstimadoMinutos { get; set; }

    [MaxLength(300)]
    public string? Observacoes { get; set; }

    public bool TeveDesconto { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ValorDesconto { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
