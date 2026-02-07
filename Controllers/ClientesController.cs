using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLavaJato.Web.Data;
using SistemaLavaJato.Web.Models.Entities;

namespace SistemaLavaJato.Web.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q = null)
    {
        var query = _context.Clientes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var termo = q.Trim();
            query = query.Where(c =>
                c.Nome.Contains(termo) ||
                (c.Cpf != null && c.Cpf.Contains(termo)) ||
                (c.Telefone != null && c.Telefone.Contains(termo)) ||
                (c.Email != null && c.Email.Contains(termo)));
        }

        var clientes = await query
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .ToListAsync();

        ViewData["Search"] = q;
        return View(clientes);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var cliente = await _context.Clientes
            .Include(c => c.Veiculos)
            .Include(c => c.Agendamentos)
                .ThenInclude(a => a.Servico)
            .Include(c => c.Agendamentos)
                .ThenInclude(a => a.Veiculo)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (cliente is null) return NotFound();

        return View(cliente);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Cpf,Telefone,Email")] Cliente cliente)
    {
        if (!ModelState.IsValid)
        {
            return View(cliente);
        }

        _context.Add(cliente);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Cpf,Telefone,Email")] Cliente cliente)
    {
        if (id != cliente.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(cliente);
        }

        try
        {
            _context.Update(cliente);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClienteExists(cliente.Id))
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

        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (cliente is null) return NotFound();

        return View(cliente);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null) return RedirectToAction(nameof(Index));

        var hasAgendamentos = await _context.Agendamentos.AnyAsync(a => a.ClienteId == id);
        var hasVeiculos = await _context.Veiculos.AnyAsync(v => v.ClienteId == id);
        if (hasAgendamentos || hasVeiculos)
        {
            ModelState.AddModelError(string.Empty, "Não é possível excluir este cliente porque existem veículos ou agendamentos vinculados.");
            return View(cliente);
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ClienteExists(int id)
    {
        return _context.Clientes.Any(e => e.Id == id);
    }
}
