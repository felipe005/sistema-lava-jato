using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaLavaJato.Web.Data;
using SistemaLavaJato.Web.Models.Entities;

namespace SistemaLavaJato.Web.Controllers;

[Authorize]
public class VeiculosController : Controller
{
    private readonly AppDbContext _context;

    public VeiculosController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var veiculos = await _context.Veiculos
            .Include(v => v.Cliente)
            .AsNoTracking()
            .OrderBy(v => v.Placa)
            .ToListAsync();

        return View(veiculos);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var veiculo = await _context.Veiculos
            .Include(v => v.Cliente)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (veiculo is null) return NotFound();

        return View(veiculo);
    }

    public IActionResult Create()
    {
        ViewData["ClienteId"] = new SelectList(_context.Clientes.OrderBy(c => c.Nome), "Id", "Nome");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ClienteId,Placa,Modelo,Marca,Cor,Ano,Observacoes")] Veiculo veiculo)
    {
        if (!ModelState.IsValid)
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes.OrderBy(c => c.Nome), "Id", "Nome", veiculo.ClienteId);
            return View(veiculo);
        }

        _context.Add(veiculo);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var veiculo = await _context.Veiculos.FindAsync(id);
        if (veiculo is null) return NotFound();

        ViewData["ClienteId"] = new SelectList(_context.Clientes.OrderBy(c => c.Nome), "Id", "Nome", veiculo.ClienteId);
        return View(veiculo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,Placa,Modelo,Marca,Cor,Ano,Observacoes")] Veiculo veiculo)
    {
        if (id != veiculo.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes.OrderBy(c => c.Nome), "Id", "Nome", veiculo.ClienteId);
            return View(veiculo);
        }

        try
        {
            _context.Update(veiculo);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!VeiculoExists(veiculo.Id))
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

        var veiculo = await _context.Veiculos
            .Include(v => v.Cliente)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (veiculo is null) return NotFound();

        return View(veiculo);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var veiculo = await _context.Veiculos.FindAsync(id);
        if (veiculo is null) return RedirectToAction(nameof(Index));

        var hasAgendamentos = await _context.Agendamentos.AnyAsync(a => a.VeiculoId == id);
        if (hasAgendamentos)
        {
            ModelState.AddModelError(string.Empty, "Não é possível excluir este veículo porque existem agendamentos vinculados.");
            var veiculoComCliente = await _context.Veiculos
                .Include(v => v.Cliente)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);
            return View(veiculoComCliente ?? veiculo);
        }

        _context.Veiculos.Remove(veiculo);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool VeiculoExists(int id)
    {
        return _context.Veiculos.Any(e => e.Id == id);
    }
}
