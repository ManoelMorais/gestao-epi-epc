using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoEpiEpc.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cargo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cargo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categoria_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categoria_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "motivo_movimentacao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tipo_aplicavel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    sem_devolucao = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_motivo_movimentacao", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unidade",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    sigla = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unidade", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "item_epi_epc",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_ca = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    validade_padrao_meses = table.Column<int>(type: "integer", nullable: true),
                    possui_tamanho = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_epi_epc", x => x.id);
                    table.ForeignKey(
                        name: "fk_item_epi_epc_categoria_item_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categoria_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "colaborador",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    drt = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    cargo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    area = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    admissao = table.Column<DateOnly>(type: "date", nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    senha_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_colaborador", x => x.id);
                    table.ForeignKey(
                        name: "fk_colaborador_cargo_cargo_id",
                        column: x => x.cargo_id,
                        principalTable: "cargo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_colaborador_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    perfil = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_usuario", x => x.id);
                    table.ForeignKey(
                        name: "fk_usuario_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cargo_item_permitido",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cargo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cargo_item_permitido", x => x.id);
                    table.ForeignKey(
                        name: "fk_cargo_item_permitido_cargo_cargo_id",
                        column: x => x.cargo_id,
                        principalTable: "cargo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cargo_item_permitido_item_epi_epc_item_id",
                        column: x => x.item_id,
                        principalTable: "item_epi_epc",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entrega",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    facilitador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entrega_origem_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_hora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    tipo_movimentacao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    assinatura_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    observacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrega", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrega_colaborador_colaborador_id",
                        column: x => x.colaborador_id,
                        principalTable: "colaborador",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entrega_entrega_entrega_origem_id",
                        column: x => x.entrega_origem_id,
                        principalTable: "entrega",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entrega_motivo_movimentacao_motivo_id",
                        column: x => x.motivo_id,
                        principalTable: "motivo_movimentacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entrega_unidade_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidade",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_entrega_usuario_facilitador_id",
                        column: x => x.facilitador_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "log_auditoria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidade = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    entidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dados_antes = table.Column<string>(type: "text", nullable: true),
                    dados_depois = table.Column<string>(type: "text", nullable: true),
                    data_hora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_log_auditoria", x => x.id);
                    table.ForeignKey(
                        name: "fk_log_auditoria_usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "entrega_item",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entrega_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    tamanho = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    numero_serie = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    foto_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    validade_calculada = table.Column<DateOnly>(type: "date", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_entrega_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_entrega_item_entrega_entrega_id",
                        column: x => x.entrega_id,
                        principalTable: "entrega",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_entrega_item_item_epi_epc_item_id",
                        column: x => x.item_id,
                        principalTable: "item_epi_epc",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitacao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocolo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    colaborador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motivo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tamanho = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    quantidade = table.Column<int>(type: "integer", nullable: false),
                    material_data_entrega = table.Column<DateOnly>(type: "date", nullable: true),
                    material_fabricacao = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    material_marca = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    material_lote = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    relato = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    fotos = table.Column<List<string>>(type: "text[]", nullable: false),
                    assinatura = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    criada_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    entrega_id = table.Column<Guid>(type: "uuid", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitacao", x => x.id);
                    table.ForeignKey(
                        name: "fk_solicitacao_colaborador_colaborador_id",
                        column: x => x.colaborador_id,
                        principalTable: "colaborador",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitacao_entrega_entrega_id",
                        column: x => x.entrega_id,
                        principalTable: "entrega",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitacao_item_epi_epc_item_id",
                        column: x => x.item_id,
                        principalTable: "item_epi_epc",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitacao_motivo_movimentacao_motivo_id",
                        column: x => x.motivo_id,
                        principalTable: "motivo_movimentacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "evento_solicitacao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    responsavel = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    atualizado_em = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_evento_solicitacao", x => x.id);
                    table.ForeignKey(
                        name: "fk_evento_solicitacao_solicitacao_solicitacao_id",
                        column: x => x.solicitacao_id,
                        principalTable: "solicitacao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cargo_nome",
                table: "cargo",
                column: "nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cargo_item_permitido_cargo_id_item_id",
                table: "cargo_item_permitido",
                columns: new[] { "cargo_id", "item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cargo_item_permitido_item_id",
                table: "cargo_item_permitido",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_colaborador_cargo_id",
                table: "colaborador",
                column: "cargo_id");

            migrationBuilder.CreateIndex(
                name: "ix_colaborador_drt",
                table: "colaborador",
                column: "drt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_colaborador_unidade_id",
                table: "colaborador",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrega_colaborador_id",
                table: "entrega",
                column: "colaborador_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrega_data_hora",
                table: "entrega",
                column: "data_hora");

            migrationBuilder.CreateIndex(
                name: "ix_entrega_entrega_origem_id",
                table: "entrega",
                column: "entrega_origem_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrega_facilitador_id",
                table: "entrega",
                column: "facilitador_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrega_motivo_id",
                table: "entrega",
                column: "motivo_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrega_unidade_id",
                table: "entrega",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrega_item_entrega_id",
                table: "entrega_item",
                column: "entrega_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrega_item_item_id",
                table: "entrega_item",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_evento_solicitacao_solicitacao_id",
                table: "evento_solicitacao",
                column: "solicitacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_epi_epc_categoria_id",
                table: "item_epi_epc",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_epi_epc_codigo",
                table: "item_epi_epc",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_log_auditoria_data_hora",
                table: "log_auditoria",
                column: "data_hora");

            migrationBuilder.CreateIndex(
                name: "ix_log_auditoria_usuario_id",
                table: "log_auditoria",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_colaborador_id",
                table: "solicitacao",
                column: "colaborador_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_criada_em",
                table: "solicitacao",
                column: "criada_em");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_entrega_id",
                table: "solicitacao",
                column: "entrega_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_item_id",
                table: "solicitacao",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_motivo_id",
                table: "solicitacao",
                column: "motivo_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_protocolo",
                table: "solicitacao",
                column: "protocolo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitacao_status",
                table: "solicitacao",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_unidade_id",
                table: "usuario",
                column: "unidade_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cargo_item_permitido");

            migrationBuilder.DropTable(
                name: "entrega_item");

            migrationBuilder.DropTable(
                name: "evento_solicitacao");

            migrationBuilder.DropTable(
                name: "log_auditoria");

            migrationBuilder.DropTable(
                name: "solicitacao");

            migrationBuilder.DropTable(
                name: "entrega");

            migrationBuilder.DropTable(
                name: "item_epi_epc");

            migrationBuilder.DropTable(
                name: "colaborador");

            migrationBuilder.DropTable(
                name: "motivo_movimentacao");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "categoria_item");

            migrationBuilder.DropTable(
                name: "cargo");

            migrationBuilder.DropTable(
                name: "unidade");
        }
    }
}
