using System.ComponentModel.DataAnnotations;

namespace SistemaLavaJato.Web.Models.Entities;

public class Veiculo
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [Required]
    [MaxLength(10)]
    public string Placa { get; set; } = string.Empty;

    [MaxLength(60)]
    public string? Modelo { get; set; }

    [MaxLength(60)]
    public string? Marca { get; set; }

    [MaxLength(30)]
    public string? Cor { get; set; }

    public int? Ano { get; set; }

    [MaxLength(200)]
    public string? Observacoes { get; set; }

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
