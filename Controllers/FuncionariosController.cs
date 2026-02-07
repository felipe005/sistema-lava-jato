using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLavaJato.Web.Data;
using SistemaLavaJato.Web.Models.Entities;
using SistemaLavaJato.Web.Models.ViewModels;

namespace SistemaLavaJato.Web.Controllers;

[Authorize(Roles = "Admin")]
public class FuncionariosController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FuncionariosController(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var funcionarios = await _context.Funcionarios
            .Include(f => f.ApplicationUser)
            .AsNoTracking()
            .OrderBy(f => f.Nome)
            .ToListAsync();

        return View(funcionarios);
    }

    public IActionResult Create()
    {
        return View(new FuncionarioCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FuncionarioCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            NomeCompleto = model.Nome
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, DbInitializer.RoleFuncionario);

        var funcionario = new Funcionario
        {
            Nome = model.Nome,
            Telefone = model.Telefone,
            SalarioMensal = model.SalarioMensal,
            PercentualComissao = model.PercentualComissao,
            ApplicationUserId = user.Id,
            Ativo = true
        };

        _context.Funcionarios.Add(funcionario);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var funcionario = await _context.Funcionarios
            .Include(f => f.ApplicationUser)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (funcionario is null) return NotFound();

        var model = new FuncionarioEditViewModel
        {
            Id = funcionario.Id,
            Nome = funcionario.Nome,
            Telefone = funcionario.Telefone,
            Email = funcionario.ApplicationUser?.Email,
            SalarioMensal = funcionario.SalarioMensal,
            PercentualComissao = funcionario.PercentualComissao,
            Ativo = funcionario.Ativo
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FuncionarioEditViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var funcionario = await _context.Funcionarios
            .Include(f => f.ApplicationUser)
            .FirstOrDefaultAsync(f => f.Id == id);
        if (funcionario is null) return NotFound();

        funcionario.Nome = model.Nome;
        funcionario.Telefone = model.Telefone;
        funcionario.SalarioMensal = model.SalarioMensal;
        funcionario.PercentualComissao = model.PercentualComissao;
        funcionario.Ativo = model.Ativo;

        if (funcionario.ApplicationUser is not null && !string.Equals(funcionario.ApplicationUser.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            funcionario.ApplicationUser.Email = model.Email;
            funcionario.ApplicationUser.UserName = model.Email;
            funcionario.ApplicationUser.NomeCompleto = model.Nome;
            _context.Update(funcionario.ApplicationUser);
        }

        _context.Update(funcionario);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var funcionario = await _context.Funcionarios
            .Include(f => f.ApplicationUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (funcionario is null) return NotFound();

        return View(funcionario);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var funcionario = await _context.Funcionarios.FindAsync(id);
        if (funcionario is null) return RedirectToAction(nameof(Index));

        _context.Funcionarios.Remove(funcionario);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
