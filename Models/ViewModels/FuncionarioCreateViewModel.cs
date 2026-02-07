using System.ComponentModel.DataAnnotations;

namespace SistemaLavaJato.Web.Models.ViewModels;

public class FuncionarioCreateViewModel
{
    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public decimal SalarioMensal { get; set; }

    public decimal PercentualComissao { get; set; }
}
