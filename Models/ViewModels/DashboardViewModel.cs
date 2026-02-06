using SistemaLavaJato.Web.Models.Entities;

namespace SistemaLavaJato.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalClientes { get; set; }
    public int TotalVeiculos { get; set; }
    public int TotalServicos { get; set; }
    public int TotalAgendamentos { get; set; }

    public IReadOnlyDictionary<StatusServico, int> StatusCounts { get; set; } =
        new Dictionary<StatusServico, int>();
}
