using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaLavaJato.Web.Data;
using SistemaLavaJato.Web.Models.Entities;

namespace SistemaLavaJato.Web.Controllers;

[Authorize]
public class AgendamentosController : Controller
{
    private readonly AppDbContext _context;

    public AgendamentosController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var agendamentos = await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Veiculo)
            .Include(a => a.Servico)
            .AsNoTracking()
            .OrderByDescending(a => a.DataAgendada)
            .ToListAsync();

        return View(agendamentos);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var agendamento = await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Veiculo)
            .Include(a => a.Servico)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (agendamento is null) return NotFound();

        return View(agendamento);
    }

    public IActionResult Create()
    {
        LoadSelectLists();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ClienteId,VeiculoId,ServicoId,DataAgendada,Status,Observacoes")] Agendamento agendamento)
    {
        if (!ModelState.IsValid)
        {
            LoadSelectLists(agendamento);
            return View(agendamento);
        }

        _context.Add(agendamento);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var agendamento = await _context.Agendamentos.FindAsync(id);
        if (agendamento is null) return NotFound();

        LoadSelectLists(agendamento);
        return View(agendamento);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,VeiculoId,ServicoId,DataAgendada,Status,Observacoes")] Agendamento agendamento)
    {
        if (id != agendamento.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            LoadSelectLists(agendamento);
            return View(agendamento);
        }

        try
        {
            _context.Update(agendamento);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AgendamentoExists(agendamento.Id))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var agendamento = await _context.Agendamentos
            .Include(a => a.Cliente)
            .Include(a => a.Veiculo)
            .Include(a => a.Servico)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (agendamento is null) return NotFound();

        return View(agendamento);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var agendamento = await _context.Agendamentos.FindAsync(id);
        if (agendamento is null) return RedirectToAction(nameof(Index));

        _context.Agendamentos.Remove(agendamento);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void LoadSelectLists(Agendamento? agendamento = null)
    {
        ViewData["ClienteId"] = new SelectList(_context.Clientes.OrderBy(c => c.Nome), "Id", "Nome", agendamento?.ClienteId);
        ViewData["VeiculoId"] = new SelectList(_context.Veiculos.OrderBy(v => v.Placa), "Id", "Placa", agendamento?.VeiculoId);
        ViewData["ServicoId"] = new SelectList(_context.Servicos.OrderBy(s => s.Nome), "Id", "Nome", agendamento?.ServicoId);
    }

    private bool AgendamentoExists(int id)
    {
        return _context.Agendamentos.Any(e => e.Id == id);
    }
}
