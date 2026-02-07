using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLavaJato.Web.Data;
using SistemaLavaJato.Web.Models.Entities;

namespace SistemaLavaJato.Web.Controllers;

[Authorize(Roles = "Admin")]
public class EstoqueController : Controller
{
    private readonly AppDbContext _context;

    public EstoqueController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var produtos = await _context.ProdutosEstoque
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync();

        var alertas = produtos.Where(p => p.QuantidadeAtual <= p.QuantidadeMinima).ToList();
        ViewData["Alertas"] = alertas;
        return View(produtos);
    }

    public IActionResult Create()
    {
        return View(new ProdutoEstoque());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Unidade,QuantidadeAtual,QuantidadeMinima,CustoUnitario")] ProdutoEstoque produto)
    {
        if (!ModelState.IsValid)
        {
            return View(produto);
        }

        _context.ProdutosEstoque.Add(produto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var produto = await _context.ProdutosEstoque.FindAsync(id);
        if (produto is null) return NotFound();

        return View(produto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Unidade,QuantidadeAtual,QuantidadeMinima,CustoUnitario")] ProdutoEstoque produto)
    {
        if (id != produto.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(produto);
        }

        _context.Update(produto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Movimentar(int? id)
    {
        if (id is null) return NotFound();

        var produto = await _context.ProdutosEstoque.FindAsync(id);
        if (produto is null) return NotFound();

        ViewData["Produto"] = produto;
        return View(new MovimentacaoEstoque { ProdutoEstoqueId = produto.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Movimentar([Bind("ProdutoEstoqueId,Tipo,Quantidade,Data,Observacoes")] MovimentacaoEstoque mov)
    {
        var produto = await _context.ProdutosEstoque.FindAsync(mov.ProdutoEstoqueId);
        if (produto is null) return NotFound();

        mov.Data = NormalizeToUtc(mov.Data);

        if (!ModelState.IsValid)
        {
            ViewData["Produto"] = produto;
            return View(mov);
        }

        if (mov.Tipo == TipoMovimentacaoEstoque.Saida && mov.Quantidade > produto.QuantidadeAtual)
        {
            ModelState.AddModelError(string.Empty, "Quantidade de saida maior que o estoque atual.");
            ViewData["Produto"] = produto;
            return View(mov);
        }

        if (mov.Tipo == TipoMovimentacaoEstoque.Entrada)
        {
            produto.QuantidadeAtual += mov.Quantidade;
        }
        else
        {
            produto.QuantidadeAtual -= mov.Quantidade;
        }

        _context.MovimentacoesEstoque.Add(mov);
        _context.ProdutosEstoque.Update(produto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Movimentacoes(int? id)
    {
        if (id is null) return NotFound();

        var produto = await _context.ProdutosEstoque.FindAsync(id);
        if (produto is null) return NotFound();

        var movimentos = await _context.MovimentacoesEstoque
            .AsNoTracking()
            .Where(m => m.ProdutoEstoqueId == id)
            .OrderByDescending(m => m.Data)
            .ToListAsync();

        ViewData["Produto"] = produto;
        return View(movimentos);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var produto = await _context.ProdutosEstoque
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        if (produto is null) return NotFound();

        return View(produto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var produto = await _context.ProdutosEstoque.FindAsync(id);
        if (produto is null) return RedirectToAction(nameof(Index));

        _context.ProdutosEstoque.Remove(produto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
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
