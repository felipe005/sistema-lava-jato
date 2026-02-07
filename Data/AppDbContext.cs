using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaLavaJato.Web.Models.Entities;

namespace SistemaLavaJato.Web.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Servico> Servicos => Set<Servico>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<GastoProduto> GastosProdutos => Set<GastoProduto>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<ProdutoEstoque> ProdutosEstoque => Set<ProdutoEstoque>();
    public DbSet<MovimentacaoEstoque> MovimentacoesEstoque => Set<MovimentacaoEstoque>();
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cliente>()
            .HasIndex(c => c.Cpf)
            .IsUnique();

        builder.Entity<Veiculo>()
            .HasIndex(v => v.Placa)
            .IsUnique();

        builder.Entity<Agendamento>()
            .HasOne(a => a.Cliente)
            .WithMany(c => c.Agendamentos)
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Agendamento>()
            .HasOne(a => a.Veiculo)
            .WithMany(v => v.Agendamentos)
            .HasForeignKey(a => a.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Agendamento>()
            .HasOne(a => a.Servico)
            .WithMany(s => s.Agendamentos)
            .HasForeignKey(a => a.ServicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GastoProduto>()
            .HasIndex(g => g.Produto);

        builder.Entity<Funcionario>()
            .HasIndex(f => f.Nome);

        builder.Entity<Agendamento>()
            .HasOne(a => a.Funcionario)
            .WithMany(f => f.Agendamentos)
            .HasForeignKey(a => a.FuncionarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ProdutoEstoque>()
            .HasIndex(p => p.Nome);

        builder.Entity<MovimentacaoEstoque>()
            .HasOne(m => m.ProdutoEstoque)
            .WithMany(p => p.Movimentacoes)
            .HasForeignKey(m => m.ProdutoEstoqueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
