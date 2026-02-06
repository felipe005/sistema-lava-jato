using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLavaJato.Web.Data;
using SistemaLavaJato.Web.Models.Entities;

namespace SistemaLavaJato.Web.Controllers;

[Authorize]
public class ServicosController : Controller
{
    private readonly AppDbContext _context;

    public ServicosController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var servicos = await _context.Servicos
            .AsNoTracking()
            .OrderBy(s => s.Nome)
            .ToListAsync();

        return View(servicos);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var servico = await _context.Servicos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (servico is null) return NotFound();

        return View(servico);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Descricao,Preco")] Servico servico)
    {
        if (!ModelState.IsValid)
        {
            return View(servico);
        }

        _context.Add(servico);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var servico = await _context.Servicos.FindAsync(id);
        if (servico is null) return NotFound();

        return View(servico);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Descricao,Preco")] Servico servico)
    {
        if (id != servico.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(servico);
        }

        try
        {
            _context.Update(servico);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ServicoExists(servico.Id))
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

        var servico = await _context.Servicos
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (servico is null) return NotFound();

        return View(servico);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var servico = await _context.Servicos.FindAsync(id);
        if (servico is null) return RedirectToAction(nameof(Index));

        _context.Servicos.Remove(servico);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ServicoExists(int id)
    {
        return _context.Servicos.Any(e => e.Id == id);
    }
}
