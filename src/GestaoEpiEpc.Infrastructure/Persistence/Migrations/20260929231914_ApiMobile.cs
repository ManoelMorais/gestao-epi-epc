using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoEpiEpc.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ApiMobile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo",
                table: "motivo_movimentacao",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                table: "categoria_item",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            // Bancos já populados (ex.: o Supabase) recebem os mesmos códigos do seed do app.
            migrationBuilder.Sql("""
                UPDATE categoria_item SET codigo = CASE nome
                    WHEN 'Proteção da Cabeça' THEN 'cat-cabeca'
                    WHEN 'Proteção das Mãos' THEN 'cat-maos'
                    WHEN 'Proteção contra Quedas' THEN 'cat-quedas'
                    WHEN 'Vestimenta' THEN 'cat-vestimenta'
                    WHEN 'Proteção Auditiva' THEN 'cat-auditiva'
                    WHEN 'Proteção dos Pés' THEN 'cat-pes'
                    WHEN 'Sinalização e Isolamento' THEN 'cat-sinalizacao'
                    ELSE 'cat-' || left(id::text, 8) END
                WHERE codigo = '';

                UPDATE motivo_movimentacao SET codigo = CASE descricao
                    WHEN 'Novo colaborador' THEN 'mot-novo'
                    WHEN 'Desgaste natural' THEN 'mot-desgaste'
                    WHEN 'Dano em serviço' THEN 'mot-avaria'
                    WHEN 'Validade vencida' THEN 'mot-validade'
                    WHEN 'Tamanho inadequado' THEN 'mot-tamanho'
                    WHEN 'Defeito de fabricação' THEN 'mot-defeito'
                    WHEN 'Perda ou extravio' THEN 'mot-perda'
                    WHEN 'Devolução por desligamento' THEN 'mot-desligamento'
                    ELSE 'mot-' || left(id::text, 8) END
                WHERE codigo = '';
                """);

            migrationBuilder.CreateIndex(
                name: "ix_motivo_movimentacao_codigo",
                table: "motivo_movimentacao",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categoria_item_codigo",
                table: "categoria_item",
                column: "codigo",
                unique: true);

            // Funções RPC que o app mobile chama (login, resumo, solicitações...) — ver Persistence/Sql/ApiMobile.sql.
            migrationBuilder.Sql(ScriptSql.Ler("ApiMobile.sql"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS public.app_login(text, text), public.app_logout(text), public.app_itens_em_posse(text),
                    public.app_listar_solicitacoes(text, text, text), public.app_obter_solicitacao(text, uuid),
                    public.app_resumo(text), public.app_itens_elegiveis(text), public.app_motivos(),
                    public.app_criar_solicitacao(text, jsonb);
                DROP SCHEMA IF EXISTS app_privado CASCADE;
                DROP TABLE IF EXISTS sessao_app;
                """);

            migrationBuilder.DropIndex(
                name: "ix_motivo_movimentacao_codigo",
                table: "motivo_movimentacao");

            migrationBuilder.DropIndex(
                name: "ix_categoria_item_codigo",
                table: "categoria_item");

            migrationBuilder.DropColumn(
                name: "codigo",
                table: "motivo_movimentacao");

            migrationBuilder.DropColumn(
                name: "codigo",
                table: "categoria_item");
        }
    }
}
