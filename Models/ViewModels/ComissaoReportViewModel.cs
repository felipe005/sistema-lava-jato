namespace SistemaLavaJato.Web.Models.ViewModels;

public class ComissaoReportViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public List<ItemComissao> Itens { get; set; } = new();

    public record ItemComissao(int FuncionarioId, string Nome, decimal Percentual, decimal TotalBase, decimal TotalComissao);
}
