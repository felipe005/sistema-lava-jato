using SistemaLavaJato.Web.Models.Entities;

namespace SistemaLavaJato.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalClientes { get; set; }
    public int TotalVeiculos { get; set; }
    public int TotalServicos { get; set; }
    public int TotalAgendamentos { get; set; }

    public decimal ReceitaDia { get; set; }
    public decimal ReceitaSemana { get; set; }
    public decimal ReceitaMes { get; set; }

    public decimal GastosDia { get; set; }
    public decimal GastosSemana { get; set; }
    public decimal GastosMes { get; set; }

    public decimal SalariosMes { get; set; }

    public decimal LucroDia => ReceitaDia - GastosDia;
    public decimal LucroSemana => ReceitaSemana - GastosSemana;
    public decimal LucroMes => ReceitaMes - (GastosMes + SalariosMes);

    public IReadOnlyDictionary<StatusServico, int> StatusCounts { get; set; } =
        new Dictionary<StatusServico, int>();
}
