using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLavaJato.Web.Data;
using SistemaLavaJato.Web.Models.Entities;
using System.Text.Json;

namespace SistemaLavaJato.Web.Controllers;

[Authorize(Roles = "Admin")]
public class GastosController : Controller
{
    private readonly AppDbContext _context;

    public GastosController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var gastos = await _context.GastosProdutos
            .AsNoTracking()
            .OrderByDescending(g => g.Data)
            .ToListAsync();

        var gastosPorProduto = await _context.GastosProdutos
            .AsNoTracking()
            .GroupBy(g => g.Produto)
            .Select(g => new { Produto = g.Key, Total = g.Sum(x => x.ValorUnitario * x.Quantidade) })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        ViewData["GastosPorProduto"] = JsonSerializer.Serialize(gastosPorProduto);

        return View(gastos);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var gasto = await _context.GastosProdutos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (gasto is null) return NotFound();

        return View(gasto);
    }

    public IActionResult Create()
    {
        return View(new GastoProduto { Data = DateTime.UtcNow });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Produto,ValorUnitario,Quantidade,Data,Observacoes")] GastoProduto gasto)
    {
        gasto.Data = NormalizeToUtc(gasto.Data);

        if (!ModelState.IsValid)
        {
            return View(gasto);
        }

        _context.Add(gasto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var gasto = await _context.GastosProdutos.FindAsync(id);
        if (gasto is null) return NotFound();

        return View(gasto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Produto,ValorUnitario,Quantidade,Data,Observacoes")] GastoProduto gasto)
    {
        if (id != gasto.Id) return NotFound();

        gasto.Data = NormalizeToUtc(gasto.Data);

        if (!ModelState.IsValid)
        {
            return View(gasto);
        }

        try
        {
            _context.Update(gasto);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GastoExists(gasto.Id))
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

        var gasto = await _context.GastosProdutos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (gasto is null) return NotFound();

        return View(gasto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var gasto = await _context.GastosProdutos.FindAsync(id);
        if (gasto is null) return RedirectToAction(nameof(Index));

        _context.GastosProdutos.Remove(gasto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool GastoExists(int id)
    {
        return _context.GastosProdutos.Any(e => e.Id == id);
    }

    private static DateTime NormalizeToUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Unspecified)
        {
            value = DateTime.SpecifyKind(value, DateTimeKind.Local);
        }

        return value.ToUniversalTime();
    }
}
