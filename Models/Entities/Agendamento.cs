using System.ComponentModel.DataAnnotations;

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

    [Required]
    public DateTime DataAgendada { get; set; }

    public StatusServico Status { get; set; } = StatusServico.Pendente;

    [MaxLength(300)]
    public string? Observacoes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
