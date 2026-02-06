using System.ComponentModel.DataAnnotations;

namespace SistemaLavaJato.Web.Models.Entities;

public class Cliente
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(14)]
    public string? Cpf { get; set; }

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [MaxLength(120)]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
