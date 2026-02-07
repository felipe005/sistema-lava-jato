using Microsoft.EntityFrameworkCore;
using SistemaLavaJato.Web.Data;
using SistemaLavaJato.Web.Models.Entities;
using SistemaLavaJato.Web.Models.ViewModels;

namespace SistemaLavaJato.Web.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var nowUtc = DateTime.UtcNow;
        var startOfDay = DateTime.SpecifyKind(nowUtc.Date, DateTimeKind.Utc);
        var startOfWeek = GetStartOfWeekUtc(nowUtc);
        var startOfMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var statusCounts = await _context.Agendamentos
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var dict = statusCounts.ToDictionary(x => x.Status, x => x.Count);

        foreach (var status in Enum.GetValues<StatusServico>())
        {
            if (!dict.ContainsKey(status))
            {
                dict[status] = 0;
            }
        }

        return new DashboardViewModel
        {
            TotalClientes = await _context.Clientes.CountAsync(),
            TotalVeiculos = await _context.Veiculos.CountAsync(),
            TotalServicos = await _context.Servicos.CountAsync(),
            TotalAgendamentos = await _context.Agendamentos.CountAsync(),
            ReceitaDia = await GetReceitaAsync(startOfDay),
            ReceitaSemana = await GetReceitaAsync(startOfWeek),
            ReceitaMes = await GetReceitaAsync(startOfMonth),
            GastosDia = await GetGastosAsync(startOfDay),
            GastosSemana = await GetGastosAsync(startOfWeek),
            GastosMes = await GetGastosAsync(startOfMonth),
            SalariosMes = await GetSalariosMesAsync(),
            StatusCounts = dict
        };
    }

    private async Task<decimal> GetReceitaAsync(DateTime startUtc)
    {
        return await _context.Agendamentos
            .AsNoTracking()
            .Where(a => a.Status == StatusServico.Concluido && a.DataAgendada >= startUtc)
            .SumAsync(a =>
                a.Servico != null
                    ? Math.Max(0, a.Servico.Preco - (a.TeveDesconto ? a.ValorDesconto : 0))
                    : 0);
    }

    private async Task<decimal> GetGastosAsync(DateTime startUtc)
    {
        return await _context.GastosProdutos
            .AsNoTracking()
            .Where(g => g.Data >= startUtc)
            .SumAsync(g => g.ValorUnitario * g.Quantidade);
    }

    private async Task<decimal> GetSalariosMesAsync()
    {
        return await _context.Funcionarios
            .AsNoTracking()
            .Where(f => f.Ativo)
            .SumAsync(f => f.SalarioMensal);
    }

    private static DateTime GetStartOfWeekUtc(DateTime nowUtc)
    {
        var day = (int)nowUtc.DayOfWeek;
        var delta = (day + 6) % 7;
        return DateTime.SpecifyKind(nowUtc.Date.AddDays(-delta), DateTimeKind.Utc);
    }
}
