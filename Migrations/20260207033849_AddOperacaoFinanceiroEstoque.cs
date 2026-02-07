using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SistemaLavaJato.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddOperacaoFinanceiroEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PercentualComissao",
                table: "Funcionarios",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FuncionarioId",
                table: "Agendamentos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InicioServico",
                table: "Agendamentos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TempoEstimadoMinutos",
                table: "Agendamentos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: true),
                    AgendamentoId = table.Column<int>(type: "integer", nullable: true),
                    Canal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Mensagem = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    EnviadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificacoes_Agendamentos_AgendamentoId",
                        column: x => x.AgendamentoId,
                        principalTable: "Agendamentos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notificacoes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProdutosEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Unidade = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    QuantidadeAtual = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeMinima = table.Column<int>(type: "integer", nullable: false),
                    CustoUnitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutosEstoque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProdutoEstoqueId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_ProdutosEstoque_ProdutoEstoqueId",
                        column: x => x.ProdutoEstoqueId,
                        principalTable: "ProdutosEstoque",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agendamentos_FuncionarioId",
                table: "Agendamentos",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ProdutoEstoqueId",
                table: "MovimentacoesEstoque",
                column: "ProdutoEstoqueId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_AgendamentoId",
                table: "Notificacoes",
                column: "AgendamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_ClienteId",
                table: "Notificacoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutosEstoque_Nome",
                table: "ProdutosEstoque",
                column: "Nome");

            migrationBuilder.AddForeignKey(
                name: "FK_Agendamentos_Funcionarios_FuncionarioId",
                table: "Agendamentos",
                column: "FuncionarioId",
                principalTable: "Funcionarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Agendamentos_Funcionarios_FuncionarioId",
                table: "Agendamentos");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "ProdutosEstoque");

            migrationBuilder.DropIndex(
                name: "IX_Agendamentos_FuncionarioId",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "PercentualComissao",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "FuncionarioId",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "InicioServico",
                table: "Agendamentos");

            migrationBuilder.DropColumn(
                name: "TempoEstimadoMinutos",
                table: "Agendamentos");
        }
    }
}
