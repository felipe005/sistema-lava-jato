using System.ComponentModel.DataAnnotations;

namespace SistemaLavaJato.Web.Models.Entities;

public class Notificacao
{
    public int Id { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public int? AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }

    [MaxLength(20)]
    public string Canal { get; set; } = "Simulacao";

    [MaxLength(300)]
    public string Mensagem { get; set; } = string.Empty;

    public DateTime EnviadoEm { get; set; } = DateTime.UtcNow;
}
