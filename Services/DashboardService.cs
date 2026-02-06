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
            StatusCounts = dict
        };
    }
}
