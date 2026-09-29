using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoEpiEpc.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// No Supabase, toda tabela do schema public fica acessível pela API REST pública (chave anon)
    /// enquanto o Row Level Security estiver desligado. Ligar o RLS sem nenhuma policy bloqueia esse
    /// acesso; o app desktop conecta como dono das tabelas (role postgres), que ignora o RLS.
    /// </summary>
    public partial class HabilitarRls : Migration
    {
        private static readonly string[] Tabelas =
        [
            "unidade", "cargo", "colaborador", "categoria_item", "item_epi_epc", "cargo_item_permitido",
            "motivo_movimentacao", "usuario", "entrega", "entrega_item", "log_auditoria", "solicitacao",
            "evento_solicitacao", "__EFMigrationsHistory"
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var tabela in Tabelas)
                migrationBuilder.Sql($"ALTER TABLE \"{tabela}\" ENABLE ROW LEVEL SECURITY;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var tabela in Tabelas)
                migrationBuilder.Sql($"ALTER TABLE \"{tabela}\" DISABLE ROW LEVEL SECURITY;");
        }
    }
}
