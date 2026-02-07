namespace SistemaLavaJato.Web.Models.ViewModels;

public class CaixaReportViewModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal TotalReceita { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal TotalSalarios { get; set; }
    public decimal TotalLucro => TotalReceita - (TotalGastos + TotalSalarios);

    public List<CaixaDia> Dias { get; set; } = new();

    public record CaixaDia(DateTime Data, decimal Receita, decimal Gastos, decimal Salarios)
    {
        public decimal Lucro => Receita - (Gastos + Salarios);
    }
}
