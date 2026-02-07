using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.Json;
using SistemaLavaJato.Web.Data;
using SistemaLavaJato.Web.Models.Entities;
using SistemaLavaJato.Web.Models.ViewModels;
using SistemaLavaJato.Web.Services;

namespace SistemaLavaJato.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly AppDbContext _context;

    public AdminController(IDashboardService dashboardService, AppDbContext context)
    {
        _dashboardService = dashboardService;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _dashboardService.GetDashboardAsync();
        return View(model);
    }

    public async Task<IActionResult> Caixa(DateTime? inicio = null, DateTime? fim = null)
    {
        var nowUtc = DateTime.UtcNow;
        var start = inicio?.Date ?? new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = fim?.Date.AddDays(1).AddTicks(-1) ?? new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddTicks(-1);

        start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        var agendamentos = await _context.Agendamentos
            .AsNoTracking()
            .Include(a => a.Servico)
            .Where(a => a.Status == StatusServico.Concluido && a.DataAgendada >= start && a.DataAgendada <= end)
            .ToListAsync();

        var gastos = await _context.GastosProdutos
            .AsNoTracking()
            .Where(g => g.Data >= start && g.Data <= end)
            .ToListAsync();

        var salarios = await _context.Funcionarios
            .AsNoTracking()
            .Where(f => f.Ativo)
            .ToListAsync();

        var dias = new List<CaixaReportViewModel.CaixaDia>();
        var cursor = start.Date;
        var endDate = end.Date;
        while (cursor <= endDate)
        {
            var dayStart = DateTime.SpecifyKind(cursor, DateTimeKind.Utc);
            var dayEnd = DateTime.SpecifyKind(cursor.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

            var receitaDia = agendamentos
                .Where(a => a.DataAgendada >= dayStart && a.DataAgendada <= dayEnd)
                .Sum(a => a.Servico != null ? Math.Max(0, a.Servico.Preco - (a.TeveDesconto ? a.ValorDesconto : 0)) : 0);

            var gastosDia = gastos
                .Where(g => g.Data >= dayStart && g.Data <= dayEnd)
                .Sum(g => g.ValorUnitario * g.Quantidade);

            var salariosDia = 0m;
            dias.Add(new CaixaReportViewModel.CaixaDia(cursor, receitaDia, gastosDia, salariosDia));
            cursor = cursor.AddDays(1);
        }

        var model = new CaixaReportViewModel
        {
            StartDate = start.ToLocalTime(),
            EndDate = end.ToLocalTime(),
            TotalReceita = dias.Sum(d => d.Receita),
            TotalGastos = dias.Sum(d => d.Gastos),
            TotalSalarios = salarios.Sum(s => s.SalarioMensal),
            Dias = dias
        };

        ViewData["ChartData"] = JsonSerializer.Serialize(model.Dias.Select(d => new
        {
            Data = d.Data.ToString("dd/MM"),
            Receita = d.Receita,
            Gastos = d.Gastos,
            Lucro = d.Lucro
        }));

        return View(model);
    }

    public async Task<IActionResult> Comissoes(DateTime? inicio = null, DateTime? fim = null)
    {
        var nowUtc = DateTime.UtcNow;
        var start = inicio?.Date ?? new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = fim?.Date.AddDays(1).AddTicks(-1) ?? new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddTicks(-1);

        start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        end = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        var agendamentos = await _context.Agendamentos
            .AsNoTracking()
            .Include(a => a.Servico)
            .Include(a => a.Funcionario)
            .Where(a => a.Status == StatusServico.Concluido && a.DataAgendada >= start && a.DataAgendada <= end && a.FuncionarioId != null)
            .ToListAsync();

        var itens = agendamentos
            .GroupBy(a => a.FuncionarioId!.Value)
            .Select(g =>
            {
                var funcionario = g.First().Funcionario!;
                var baseTotal = g.Sum(a => a.Servico != null ? Math.Max(0, a.Servico.Preco - (a.TeveDesconto ? a.ValorDesconto : 0)) : 0);
                var comissao = baseTotal * (funcionario.PercentualComissao / 100m);
                return new ComissaoReportViewModel.ItemComissao(funcionario.Id, funcionario.Nome, funcionario.PercentualComissao, baseTotal, comissao);
            })
            .OrderBy(i => i.Nome)
            .ToList();

        var model = new ComissaoReportViewModel
        {
            StartDate = start.ToLocalTime(),
            EndDate = end.ToLocalTime(),
            Itens = itens
        };

        return View(model);
    }

    public async Task<IActionResult> ExportarCaixaCsv()
    {
        var caixa = await GetCaixaMesAsync();
        var culture = new CultureInfo("pt-BR");

        var sb = new StringBuilder();
        sb.AppendLine("Periodo;Receita;Gastos;Salarios;Lucro");
        sb.AppendLine(string.Join(";",
            caixa.Periodo,
            caixa.Receita.ToString("F2", culture),
            caixa.Gastos.ToString("F2", culture),
            caixa.Salarios.ToString("F2", culture),
            caixa.Lucro.ToString("F2", culture)));

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"caixa-{caixa.AnoMes}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    public async Task<IActionResult> ExportarCaixaPdf()
    {
        var caixa = await GetCaixaMesAsync();
        var culture = new CultureInfo("pt-BR");
        var lines = new List<(string Label, string Value)>
        {
            ("Periodo", caixa.Periodo),
            ("Receita", SanitizePdfValue(caixa.Receita.ToString("C", culture))),
            ("Gastos", SanitizePdfValue(caixa.Gastos.ToString("C", culture))),
            ("Salarios", SanitizePdfValue(caixa.Salarios.ToString("C", culture))),
            ("Lucro", SanitizePdfValue(caixa.Lucro.ToString("C", culture)))
        };

        var bytes = SimplePdfBuilder.Build("Caixa do Mes", lines);
        var fileName = $"caixa-{caixa.AnoMes}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    private async Task<CaixaMes> GetCaixaMesAsync()
    {
        var nowUtc = DateTime.UtcNow;
        var startOfMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var receita = await _context.Agendamentos
            .AsNoTracking()
            .Where(a => a.Status == StatusServico.Concluido && a.DataAgendada >= startOfMonth)
            .SumAsync(a =>
                a.Servico != null
                    ? Math.Max(0, a.Servico.Preco - (a.TeveDesconto ? a.ValorDesconto : 0))
                    : 0);

        var gastos = await _context.GastosProdutos
            .AsNoTracking()
            .Where(g => g.Data >= startOfMonth)
            .SumAsync(g => g.ValorUnitario * g.Quantidade);

        var salarios = await _context.Funcionarios
            .AsNoTracking()
            .Where(f => f.Ativo)
            .SumAsync(f => f.SalarioMensal);

        var periodo = $"{startOfMonth:MM/yyyy}";
        var anoMes = $"{startOfMonth:yyyy-MM}";
        return new CaixaMes(periodo, anoMes, receita, gastos, salarios);
    }

    private record CaixaMes(string Periodo, string AnoMes, decimal Receita, decimal Gastos, decimal Salarios)
    {
        public decimal Lucro => Receita - (Gastos + Salarios);
    }

    private static string SanitizePdfValue(string value)
    {
        return value.Replace('\u00A0', ' ');
    }
}
