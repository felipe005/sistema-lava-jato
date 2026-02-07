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
            .Include(a => a.Funcionario)
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
            .Include(a => a.Funcionario)
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
    public async Task<IActionResult> Create(
        [Bind("ClienteId,VeiculoId,ServicoId,FuncionarioId,DataAgendada,TempoEstimadoMinutos,Status,Observacoes,TeveDesconto,ValorDesconto")] Agendamento agendamento,
        string? novoClienteNome,
        string? novoClienteTelefone,
        string? novaPlaca
    )
    {
        agendamento.DataAgendada = NormalizeToUtc(agendamento.DataAgendada);
        if (!agendamento.TeveDesconto)
        {
            agendamento.ValorDesconto = 0;
        }

        var hasNovoCliente = !string.IsNullOrWhiteSpace(novoClienteNome);
        var hasNovaPlaca = !string.IsNullOrWhiteSpace(novaPlaca);
        var placaNormalizada = hasNovaPlaca ? novaPlaca!.Trim().ToUpperInvariant() : null;

        Cliente? novoCliente = null;
        Veiculo? novoVeiculo = null;

        if (hasNovoCliente || hasNovaPlaca)
        {
            if (hasNovoCliente && hasNovaPlaca)
            {
                if (await _context.Veiculos.AnyAsync(v => v.Placa == placaNormalizada))
                {
                    ModelState.AddModelError(nameof(novaPlaca), "Esta placa já está cadastrada.");
                }
                else
                {
                    novoCliente = new Cliente
                    {
                        Nome = novoClienteNome!.Trim(),
                        Telefone = string.IsNullOrWhiteSpace(novoClienteTelefone) ? null : novoClienteTelefone.Trim()
                    };
                    novoVeiculo = new Veiculo
                    {
                        Placa = placaNormalizada!,
                        Cliente = novoCliente
                    };

                    _context.Clientes.Add(novoCliente);
                    _context.Veiculos.Add(novoVeiculo);
                }
            }
            else if (hasNovaPlaca)
            {
                if (agendamento.ClienteId <= 0)
                {
                    ModelState.AddModelError(nameof(novaPlaca), "Selecione um cliente para cadastrar uma nova placa.");
                }
                else if (await _context.Veiculos.AnyAsync(v => v.Placa == placaNormalizada))
                {
                    ModelState.AddModelError(nameof(novaPlaca), "Esta placa já está cadastrada.");
                }
                else
                {
                    novoVeiculo = new Veiculo
                    {
                        Placa = placaNormalizada!,
                        ClienteId = agendamento.ClienteId
                    };
                    _context.Veiculos.Add(novoVeiculo);
                }
            }
            else
            {
                ModelState.AddModelError(nameof(novoClienteNome), "Informe o cliente e a placa para cadastro rápido.");
                ModelState.AddModelError(nameof(novaPlaca), "Informe o cliente e a placa para cadastro rápido.");
            }
        }

        if (!ModelState.IsValid)
        {
            ViewData["NovoClienteNome"] = novoClienteNome;
            ViewData["NovaPlaca"] = novaPlaca;
            ViewData["NovoClienteTelefone"] = novoClienteTelefone;
            LoadSelectLists(agendamento);
            return View(agendamento);
        }

        if (novoCliente is not null || novoVeiculo is not null)
        {
            await _context.SaveChangesAsync();
        }

        if (novoCliente is not null)
        {
            agendamento.ClienteId = novoCliente.Id;
        }

        if (novoVeiculo is not null)
        {
            agendamento.VeiculoId = novoVeiculo.Id;
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
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,VeiculoId,ServicoId,FuncionarioId,DataAgendada,TempoEstimadoMinutos,Status,Observacoes,TeveDesconto,ValorDesconto")] Agendamento agendamento)
    {
        if (id != agendamento.Id) return NotFound();

        agendamento.DataAgendada = NormalizeToUtc(agendamento.DataAgendada);
        if (!agendamento.TeveDesconto)
        {
            agendamento.ValorDesconto = 0;
        }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Concluir(int id)
    {
        var agendamento = await _context.Agendamentos.FindAsync(id);
        if (agendamento is null) return NotFound();

        agendamento.Status = StatusServico.Concluido;
        _context.Update(agendamento);
        var cliente = await _context.Clientes.FindAsync(agendamento.ClienteId);
        if (cliente is not null)
        {
            var mensagem = $"Seu veiculo esta pronto. Agendamento #{agendamento.Id}.";
            _context.Notificacoes.Add(new Notificacao
            {
                ClienteId = cliente.Id,
                AgendamentoId = agendamento.Id,
                Canal = "Simulacao",
                Mensagem = mensagem,
                EnviadoEm = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entregar(int id)
    {
        var agendamento = await _context.Agendamentos.FindAsync(id);
        if (agendamento is null) return NotFound();

        agendamento.Status = StatusServico.Entregue;
        _context.Update(agendamento);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarServico(int id, int? tempoEstimadoMinutos)
    {
        var agendamento = await _context.Agendamentos.FindAsync(id);
        if (agendamento is null) return NotFound();

        agendamento.InicioServico = DateTime.UtcNow;
        if (tempoEstimadoMinutos.HasValue && tempoEstimadoMinutos > 0)
        {
            agendamento.TempoEstimadoMinutos = tempoEstimadoMinutos;
        }
        agendamento.Status = StatusServico.EmAndamento;
        _context.Update(agendamento);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void LoadSelectLists(Agendamento? agendamento = null)
    {
        ViewData["ClienteId"] = new SelectList(_context.Clientes.OrderBy(c => c.Nome), "Id", "Nome", agendamento?.ClienteId);
        ViewData["VeiculoId"] = new SelectList(_context.Veiculos.OrderBy(v => v.Placa), "Id", "Placa", agendamento?.VeiculoId);
        ViewData["ServicoId"] = new SelectList(_context.Servicos.OrderBy(s => s.Nome), "Id", "Nome", agendamento?.ServicoId);
        ViewData["FuncionarioId"] = new SelectList(_context.Funcionarios.Where(f => f.Ativo).OrderBy(f => f.Nome), "Id", "Nome", agendamento?.FuncionarioId);
    }

    private bool AgendamentoExists(int id)
    {
        return _context.Agendamentos.Any(e => e.Id == id);
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
