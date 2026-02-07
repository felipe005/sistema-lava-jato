using System.ComponentModel.DataAnnotations;

namespace SistemaLavaJato.Web.Models.ViewModels;

public class FuncionarioEditViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    public decimal SalarioMensal { get; set; }

    public bool Ativo { get; set; } = true;

    public decimal PercentualComissao { get; set; }
}
