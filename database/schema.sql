CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE cargo (
        id uuid NOT NULL,
        nome character varying(120) NOT NULL,
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_cargo PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE categoria_item (
        id uuid NOT NULL,
        nome character varying(120) NOT NULL,
        tipo character varying(30) NOT NULL,
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_categoria_item PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE motivo_movimentacao (
        id uuid NOT NULL,
        descricao character varying(150) NOT NULL,
        tipo_aplicavel character varying(30) NOT NULL,
        sem_devolucao boolean NOT NULL,
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_motivo_movimentacao PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE unidade (
        id uuid NOT NULL,
        nome character varying(120) NOT NULL,
        sigla character varying(10) NOT NULL,
        cidade character varying(120) NOT NULL,
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_unidade PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE item_epi_epc (
        id uuid NOT NULL,
        codigo character varying(20) NOT NULL,
        nome character varying(150) NOT NULL,
        categoria_id uuid NOT NULL,
        numero_ca character varying(20),
        validade_padrao_meses integer,
        possui_tamanho boolean NOT NULL,
        ativo boolean NOT NULL,
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_item_epi_epc PRIMARY KEY (id),
        CONSTRAINT fk_item_epi_epc_categoria_item_categoria_id FOREIGN KEY (categoria_id) REFERENCES categoria_item (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE colaborador (
        id uuid NOT NULL,
        drt character varying(20) NOT NULL,
        nome character varying(150) NOT NULL,
        cargo_id uuid NOT NULL,
        area character varying(120) NOT NULL,
        unidade_id uuid NOT NULL,
        status character varying(30) NOT NULL,
        admissao date NOT NULL,
        email character varying(150) NOT NULL,
        telefone character varying(20) NOT NULL,
        senha_hash character varying(200),
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_colaborador PRIMARY KEY (id),
        CONSTRAINT fk_colaborador_cargo_cargo_id FOREIGN KEY (cargo_id) REFERENCES cargo (id) ON DELETE RESTRICT,
        CONSTRAINT fk_colaborador_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE usuario (
        id uuid NOT NULL,
        nome character varying(150) NOT NULL,
        email character varying(150) NOT NULL,
        perfil character varying(30) NOT NULL,
        unidade_id uuid NOT NULL,
        ativo boolean NOT NULL,
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_usuario PRIMARY KEY (id),
        CONSTRAINT fk_usuario_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE cargo_item_permitido (
        id uuid NOT NULL,
        cargo_id uuid NOT NULL,
        item_id uuid NOT NULL,
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_cargo_item_permitido PRIMARY KEY (id),
        CONSTRAINT fk_cargo_item_permitido_cargo_cargo_id FOREIGN KEY (cargo_id) REFERENCES cargo (id) ON DELETE CASCADE,
        CONSTRAINT fk_cargo_item_permitido_item_epi_epc_item_id FOREIGN KEY (item_id) REFERENCES item_epi_epc (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE entrega (
        id uuid NOT NULL,
        colaborador_id uuid NOT NULL,
        facilitador_id uuid NOT NULL,
        unidade_id uuid NOT NULL,
        motivo_id uuid NOT NULL,
        entrega_origem_id uuid,
        data_hora timestamp without time zone NOT NULL,
        tipo_movimentacao character varying(30) NOT NULL,
        status character varying(30) NOT NULL,
        assinatura_url character varying(500),
        observacao character varying(1000),
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_entrega PRIMARY KEY (id),
        CONSTRAINT fk_entrega_colaborador_colaborador_id FOREIGN KEY (colaborador_id) REFERENCES colaborador (id) ON DELETE RESTRICT,
        CONSTRAINT fk_entrega_entrega_entrega_origem_id FOREIGN KEY (entrega_origem_id) REFERENCES entrega (id) ON DELETE RESTRICT,
        CONSTRAINT fk_entrega_motivo_movimentacao_motivo_id FOREIGN KEY (motivo_id) REFERENCES motivo_movimentacao (id) ON DELETE RESTRICT,
        CONSTRAINT fk_entrega_unidade_unidade_id FOREIGN KEY (unidade_id) REFERENCES unidade (id) ON DELETE RESTRICT,
        CONSTRAINT fk_entrega_usuario_facilitador_id FOREIGN KEY (facilitador_id) REFERENCES usuario (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE log_auditoria (
        id uuid NOT NULL,
        usuario_id uuid NOT NULL,
        entidade character varying(60) NOT NULL,
        entidade_id uuid NOT NULL,
        acao character varying(200) NOT NULL,
        dados_antes text,
        dados_depois text,
        data_hora timestamp without time zone NOT NULL,
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_log_auditoria PRIMARY KEY (id),
        CONSTRAINT fk_log_auditoria_usuario_usuario_id FOREIGN KEY (usuario_id) REFERENCES usuario (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE entrega_item (
        id uuid NOT NULL,
        entrega_id uuid NOT NULL,
        item_id uuid NOT NULL,
        quantidade integer NOT NULL,
        tamanho character varying(10),
        numero_serie character varying(60),
        foto_url character varying(500),
        validade_calculada date,
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_entrega_item PRIMARY KEY (id),
        CONSTRAINT fk_entrega_item_entrega_entrega_id FOREIGN KEY (entrega_id) REFERENCES entrega (id) ON DELETE CASCADE,
        CONSTRAINT fk_entrega_item_item_epi_epc_item_id FOREIGN KEY (item_id) REFERENCES item_epi_epc (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE solicitacao (
        id uuid NOT NULL,
        protocolo character varying(20) NOT NULL,
        colaborador_id uuid NOT NULL,
        item_id uuid NOT NULL,
        motivo_id uuid NOT NULL,
        tamanho character varying(10),
        quantidade integer NOT NULL,
        material_data_entrega date,
        material_fabricacao character varying(7),
        material_marca character varying(80),
        material_lote character varying(40),
        relato character varying(1000) NOT NULL,
        fotos text[] NOT NULL,
        assinatura text NOT NULL,
        status character varying(30) NOT NULL,
        criada_em timestamp without time zone NOT NULL,
        entrega_id uuid,
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_solicitacao PRIMARY KEY (id),
        CONSTRAINT fk_solicitacao_colaborador_colaborador_id FOREIGN KEY (colaborador_id) REFERENCES colaborador (id) ON DELETE RESTRICT,
        CONSTRAINT fk_solicitacao_entrega_entrega_id FOREIGN KEY (entrega_id) REFERENCES entrega (id) ON DELETE RESTRICT,
        CONSTRAINT fk_solicitacao_item_epi_epc_item_id FOREIGN KEY (item_id) REFERENCES item_epi_epc (id) ON DELETE RESTRICT,
        CONSTRAINT fk_solicitacao_motivo_movimentacao_motivo_id FOREIGN KEY (motivo_id) REFERENCES motivo_movimentacao (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE TABLE evento_solicitacao (
        id uuid NOT NULL,
        solicitacao_id uuid NOT NULL,
        status character varying(30) NOT NULL,
        data_hora timestamp without time zone NOT NULL,
        responsavel character varying(150) NOT NULL,
        comentario character varying(1000),
        criado_em timestamp without time zone NOT NULL,
        atualizado_em timestamp without time zone,
        CONSTRAINT pk_evento_solicitacao PRIMARY KEY (id),
        CONSTRAINT fk_evento_solicitacao_solicitacao_solicitacao_id FOREIGN KEY (solicitacao_id) REFERENCES solicitacao (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_cargo_nome ON cargo (nome);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_cargo_item_permitido_cargo_id_item_id ON cargo_item_permitido (cargo_id, item_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_cargo_item_permitido_item_id ON cargo_item_permitido (item_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_colaborador_cargo_id ON colaborador (cargo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_colaborador_drt ON colaborador (drt);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_colaborador_unidade_id ON colaborador (unidade_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_entrega_colaborador_id ON entrega (colaborador_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_entrega_data_hora ON entrega (data_hora);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_entrega_entrega_origem_id ON entrega (entrega_origem_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_entrega_facilitador_id ON entrega (facilitador_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_entrega_motivo_id ON entrega (motivo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_entrega_unidade_id ON entrega (unidade_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_entrega_item_entrega_id ON entrega_item (entrega_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_entrega_item_item_id ON entrega_item (item_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_evento_solicitacao_solicitacao_id ON evento_solicitacao (solicitacao_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_item_epi_epc_categoria_id ON item_epi_epc (categoria_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_item_epi_epc_codigo ON item_epi_epc (codigo);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_log_auditoria_data_hora ON log_auditoria (data_hora);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_log_auditoria_usuario_id ON log_auditoria (usuario_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_solicitacao_colaborador_id ON solicitacao (colaborador_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_solicitacao_criada_em ON solicitacao (criada_em);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_solicitacao_entrega_id ON solicitacao (entrega_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_solicitacao_item_id ON solicitacao (item_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_solicitacao_motivo_id ON solicitacao (motivo_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_solicitacao_protocolo ON solicitacao (protocolo);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_solicitacao_status ON solicitacao (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE UNIQUE INDEX ix_usuario_email ON usuario (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    CREATE INDEX ix_usuario_unidade_id ON usuario (unidade_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225421_CriacaoInicial') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260929225421_CriacaoInicial', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "unidade" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "cargo" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "colaborador" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "categoria_item" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "item_epi_epc" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "cargo_item_permitido" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "motivo_movimentacao" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "usuario" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "entrega" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "entrega_item" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "log_auditoria" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "solicitacao" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "evento_solicitacao" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    ALTER TABLE "__EFMigrationsHistory" ENABLE ROW LEVEL SECURITY;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929225427_HabilitarRls') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260929225427_HabilitarRls', '9.0.9');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929231914_ApiMobile') THEN
    ALTER TABLE motivo_movimentacao ADD codigo character varying(40) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929231914_ApiMobile') THEN
    ALTER TABLE categoria_item ADD codigo character varying(40) NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929231914_ApiMobile') THEN
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
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929231914_ApiMobile') THEN
    CREATE UNIQUE INDEX ix_motivo_movimentacao_codigo ON motivo_movimentacao (codigo);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929231914_ApiMobile') THEN
    CREATE UNIQUE INDEX ix_categoria_item_codigo ON categoria_item (codigo);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929231914_ApiMobile') THEN
    -- =====================================================================================
    -- API do app mobile (colaborador), servida pelo próprio Postgres/Supabase via RPC.
    --
    -- Por que funções no banco e não uma API ASP.NET: o APK precisa funcionar em qualquer rede,
    -- sem depender de um servidor ligado no PC. O Supabase expõe cada função de "public" em
    -- POST /rest/v1/rpc/<nome>; o app chama só essas funções com a chave publishable.
    --
    -- Segurança:
    --   * As tabelas continuam com RLS ligado e SEM policies: a chave do app não lê nenhuma tabela.
    --   * As funções públicas são SECURITY DEFINER e só devolvem dados do colaborador dono do token.
    --   * Os auxiliares ficam no schema "app_privado", que o Supabase não expõe pela API.
    --   * O token de sessão nunca é gravado em texto: guardamos o SHA-256 dele.
    --
    -- As regras espelham o SolicitacaoService/ColaboradorService do C# (e o apiEmMemoria.ts do app);
    -- os testes PostgresIntegracaoTests comparam as duas implementações.
    -- O JSON devolvido segue exatamente os tipos de mobile/src/types/dominio.ts.
    -- =====================================================================================

    CREATE SCHEMA IF NOT EXISTS extensions;
    CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA extensions;
    CREATE SCHEMA IF NOT EXISTS app_privado;
    REVOKE ALL ON SCHEMA app_privado FROM PUBLIC;

    -- ---- Sessões do app ---------------------------------------------------------------
    CREATE TABLE IF NOT EXISTS sessao_app (
        token_hash      text PRIMARY KEY,
        colaborador_id  uuid NOT NULL REFERENCES colaborador (id) ON DELETE CASCADE,
        criada_em       timestamp without time zone NOT NULL,
        expira_em       timestamp without time zone NOT NULL
    );
    CREATE INDEX IF NOT EXISTS ix_sessao_app_colaborador_id ON sessao_app (colaborador_id);
    ALTER TABLE sessao_app ENABLE ROW LEVEL SECURITY;

    -- ---- Auxiliares (privados) ------------------------------------------------------------

    -- Horário local da operação (o desktop grava DateTime.Now; o servidor do Supabase roda em UTC).
    CREATE OR REPLACE FUNCTION app_privado.agora() RETURNS timestamp without time zone
    LANGUAGE sql STABLE AS $$ SELECT (now() AT TIME ZONE 'America/Bahia')::timestamp $$;

    -- Data/hora local → ISO 8601 com fuso, para o JavaScript interpretar sem ambiguidade.
    CREATE OR REPLACE FUNCTION app_privado.iso(ts timestamp without time zone) RETURNS text
    LANGUAGE sql IMMUTABLE AS $$ SELECT to_char(ts, 'YYYY-MM-DD"T"HH24:MI:SS') || '-03:00' $$;

    -- Minúsculas e sem acento, para a busca por texto.
    CREATE OR REPLACE FUNCTION app_privado.normalizar(texto text) RETURNS text
    LANGUAGE sql IMMUTABLE AS $$
        SELECT lower(translate(coalesce(texto, ''),
            'áàâãäéèêëíìîïóòôõöúùûüçÁÀÂÃÄÉÈÊËÍÌÎÏÓÒÔÕÖÚÙÛÜÇ',
            'aaaaaeeeeiiiiooooouuuucAAAAAEEEEIIIIOOOOOUUUUC'))
    $$;

    CREATE OR REPLACE FUNCTION app_privado.hash_token(p_token text) RETURNS text
    LANGUAGE sql IMMUTABLE AS $$ SELECT encode(extensions.digest(coalesce(p_token, ''), 'sha256'), 'hex') $$;

    -- Colaborador dono do token (ou erro de negócio, exibido pelo app).
    CREATE OR REPLACE FUNCTION app_privado.colaborador_da_sessao(p_token text) RETURNS uuid
    LANGUAGE plpgsql STABLE AS $$
    DECLARE
        v_id uuid;
        v_status text;
    BEGIN
        SELECT s.colaborador_id, c.status INTO v_id, v_status
        FROM sessao_app s JOIN colaborador c ON c.id = s.colaborador_id
        WHERE s.token_hash = app_privado.hash_token(p_token) AND s.expira_em > app_privado.agora();

        IF v_id IS NULL THEN
            RAISE EXCEPTION 'Sua sessão expirou. Entre novamente.';
        END IF;
        IF v_status = 'Inativo' THEN
            RAISE EXCEPTION 'Seu acesso está desativado.';
        END IF;
        RETURN v_id;
    END $$;

    CREATE OR REPLACE FUNCTION app_privado.json_colaborador(p_id uuid) RETURNS jsonb
    LANGUAGE sql STABLE AS $$
        SELECT jsonb_build_object(
            'id', c.id, 'drt', c.drt, 'nome', c.nome, 'cargoId', c.cargo_id, 'area', c.area,
            'unidadeId', c.unidade_id, 'status', c.status, 'admissao', to_char(c.admissao, 'YYYY-MM-DD'),
            'email', c.email, 'telefone', c.telefone,
            'cargo', jsonb_build_object('id', cg.id, 'nome', cg.nome),
            'unidade', jsonb_build_object('id', u.id, 'nome', u.nome, 'sigla', u.sigla, 'cidade', u.cidade))
        FROM colaborador c
        JOIN cargo cg ON cg.id = c.cargo_id
        JOIN unidade u ON u.id = c.unidade_id
        WHERE c.id = p_id
    $$;

    -- Categoria e motivo são identificados pelo código estável ("cat-pes", "mot-validade"), que o app usa nas telas.
    CREATE OR REPLACE FUNCTION app_privado.json_item(p_id uuid) RETURNS jsonb
    LANGUAGE sql STABLE AS $$
        SELECT jsonb_build_object(
            'id', i.id, 'codigo', i.codigo, 'nome', i.nome, 'categoriaId', cat.codigo,
            'numeroCa', i.numero_ca, 'validadePadraoMeses', i.validade_padrao_meses, 'possuiTamanho', i.possui_tamanho,
            'categoria', jsonb_build_object('id', cat.codigo, 'nome', cat.nome, 'tipo', cat.tipo))
        FROM item_epi_epc i
        JOIN categoria_item cat ON cat.id = i.categoria_id
        WHERE i.id = p_id
    $$;

    CREATE OR REPLACE FUNCTION app_privado.json_motivo(p_id uuid) RETURNS jsonb
    LANGUAGE sql STABLE AS $$
        SELECT jsonb_build_object('id', m.codigo, 'descricao', m.descricao, 'tipoAplicavel', m.tipo_aplicavel, 'semDevolucao', m.sem_devolucao)
        FROM motivo_movimentacao m
        WHERE m.id = p_id
    $$;

    CREATE OR REPLACE FUNCTION app_privado.json_solicitacao(p_id uuid) RETURNS jsonb
    LANGUAGE sql STABLE AS $$
        SELECT jsonb_strip_nulls(jsonb_build_object(
                   'id', s.id, 'protocolo', s.protocolo, 'colaboradorId', s.colaborador_id,
                   'tamanho', s.tamanho, 'quantidade', s.quantidade, 'status', s.status,
                   'criadaEm', app_privado.iso(s.criada_em), 'assinatura', s.assinatura,
                   'materialAntigo', jsonb_build_object(
                       'dataEntrega', app_privado.iso(s.material_data_entrega::timestamp),
                       'fabricacao', s.material_fabricacao, 'marca', s.material_marca,
                       'lote', s.material_lote, 'relato', s.relato)))
            || jsonb_build_object(
                   'fotos', to_jsonb(coalesce(s.fotos, ARRAY[]::text[])),
                   'item', app_privado.json_item(s.item_id),
                   'motivo', app_privado.json_motivo(s.motivo_id),
                   'historico', coalesce((
                       SELECT jsonb_agg(jsonb_strip_nulls(jsonb_build_object(
                                  'status', e.status,
                                  'dataHora', app_privado.iso(e.data_hora),
                                  -- O próprio colaborador aparece como "Você" no app.
                                  'responsavel', CASE WHEN e.responsavel = c.nome THEN 'Você' ELSE e.responsavel END,
                                  'comentario', e.comentario))
                              ORDER BY e.data_hora)
                       FROM evento_solicitacao e
                       WHERE e.solicitacao_id = s.id), '[]'::jsonb))
        FROM solicitacao s
        JOIN colaborador c ON c.id = s.colaborador_id
        WHERE s.id = p_id
    $$;

    -- O que está com o colaborador: última movimentação confirmada de cada item, exceto devoluções,
    -- com a validade calculada; vencidos primeiro, itens sem validade no fim, empate pelo nome (= ObterItensEmPosseAsync).
    CREATE OR REPLACE FUNCTION app_privado.json_itens_em_posse(p_colaborador uuid) RETURNS jsonb
    LANGUAGE sql STABLE AS $$
        WITH ultima AS (
            SELECT DISTINCT ON (ei.item_id)
                   ei.item_id, ei.quantidade, ei.tamanho, e.data_hora, e.tipo_movimentacao
            FROM entrega e
            JOIN entrega_item ei ON ei.entrega_id = e.id
            WHERE e.colaborador_id = p_colaborador AND e.status = 'Confirmada'
            ORDER BY ei.item_id, e.data_hora DESC
        ), calculado AS (
            SELECT u.*, i.nome,
                   CASE WHEN i.validade_padrao_meses IS NOT NULL
                        THEN u.data_hora + make_interval(months => i.validade_padrao_meses) END AS vence
            FROM ultima u
            JOIN item_epi_epc i ON i.id = u.item_id
            WHERE u.tipo_movimentacao <> 'Devolucao'
        )
        SELECT coalesce(jsonb_agg(
                   jsonb_strip_nulls(jsonb_build_object(
                       'quantidade', quantidade, 'tamanho', tamanho,
                       'recebidoEm', app_privado.iso(data_hora), 'venceEm', app_privado.iso(vence),
                       'diasParaVencer', vence::date - app_privado.agora()::date))
                   || jsonb_build_object('item', app_privado.json_item(item_id))
                   ORDER BY vence::date NULLS LAST, nome COLLATE "C"),
               '[]'::jsonb)
        FROM calculado
    $$;

    -- ---- API pública (chamada pelo app) -----------------------------------------------------

    CREATE OR REPLACE FUNCTION public.app_login(p_drt text, p_senha text) RETURNS jsonb
    LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, extensions, pg_temp AS $$
    DECLARE
        v_colaborador colaborador;
        v_token text;
        v_agora timestamp := app_privado.agora();
    BEGIN
        SELECT * INTO v_colaborador FROM colaborador WHERE drt = trim(p_drt);

        -- Só hashes bcrypt ($2a$...) são verificáveis aqui; o desktop regrava os antigos na inicialização.
        IF v_colaborador.id IS NULL OR v_colaborador.senha_hash IS NULL OR v_colaborador.senha_hash NOT LIKE '$2%'
           OR extensions.crypt(coalesce(p_senha, ''), v_colaborador.senha_hash) <> v_colaborador.senha_hash THEN
            RAISE EXCEPTION 'DRT ou senha inválidos.';
        END IF;

        IF v_colaborador.status = 'Inativo' THEN
            RAISE EXCEPTION 'Seu acesso está desativado. Procure o RH ou o almoxarifado da sua unidade.';
        END IF;

        DELETE FROM sessao_app WHERE expira_em <= v_agora;

        v_token := encode(extensions.gen_random_bytes(32), 'hex');
        INSERT INTO sessao_app (token_hash, colaborador_id, criada_em, expira_em)
        VALUES (app_privado.hash_token(v_token), v_colaborador.id, v_agora, v_agora + interval '30 days');

        RETURN jsonb_build_object('token', v_token, 'colaborador', app_privado.json_colaborador(v_colaborador.id));
    END $$;

    CREATE OR REPLACE FUNCTION public.app_logout(p_token text) RETURNS void
    LANGUAGE sql SECURITY DEFINER SET search_path = public, extensions, pg_temp AS $$
        DELETE FROM sessao_app WHERE token_hash = app_privado.hash_token(p_token)
    $$;

    CREATE OR REPLACE FUNCTION public.app_itens_em_posse(p_token text) RETURNS jsonb
    LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, extensions, pg_temp AS $$
    BEGIN
        RETURN app_privado.json_itens_em_posse(app_privado.colaborador_da_sessao(p_token));
    END $$;

    CREATE OR REPLACE FUNCTION public.app_listar_solicitacoes(p_token text, p_grupo text DEFAULT NULL, p_termo text DEFAULT NULL) RETURNS jsonb
    LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, extensions, pg_temp AS $$
    DECLARE
        v_colaborador uuid := app_privado.colaborador_da_sessao(p_token);
        v_termo text := app_privado.normalizar(trim(coalesce(p_termo, '')));
    BEGIN
        RETURN coalesce((
            SELECT jsonb_agg(app_privado.json_solicitacao(s.id) ORDER BY s.criada_em DESC)
            FROM solicitacao s
            JOIN item_epi_epc i ON i.id = s.item_id
            WHERE s.colaborador_id = v_colaborador
              AND (coalesce(p_grupo, '') = ''
                   OR (p_grupo = 'andamento' AND s.status IN ('Pendente', 'EmAnalise'))
                   OR s.status = p_grupo)
              AND (v_termo = ''
                   OR app_privado.normalizar(i.nome) LIKE '%' || v_termo || '%'
                   OR app_privado.normalizar(s.protocolo) LIKE '%' || v_termo || '%')
        ), '[]'::jsonb);
    END $$;

    -- Nulo quando a solicitação não existe ou é de outro colaborador (o app mostra "não encontrada").
    CREATE OR REPLACE FUNCTION public.app_obter_solicitacao(p_token text, p_id uuid) RETURNS jsonb
    LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, extensions, pg_temp AS $$
    DECLARE
        v_colaborador uuid := app_privado.colaborador_da_sessao(p_token);
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM solicitacao WHERE id = p_id AND colaborador_id = v_colaborador) THEN
            RETURN NULL;
        END IF;
        RETURN app_privado.json_solicitacao(p_id);
    END $$;

    CREATE OR REPLACE FUNCTION public.app_resumo(p_token text) RETURNS jsonb
    LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, extensions, pg_temp AS $$
    DECLARE
        v_colaborador uuid := app_privado.colaborador_da_sessao(p_token);
    BEGIN
        RETURN jsonb_build_object(
            'itensEmPosse', app_privado.json_itens_em_posse(v_colaborador),
            'porStatus', (
                SELECT jsonb_build_object(
                    'Pendente', count(*) FILTER (WHERE status = 'Pendente'),
                    'EmAnalise', count(*) FILTER (WHERE status = 'EmAnalise'),
                    'Aprovada', count(*) FILTER (WHERE status = 'Aprovada'),
                    'Entregue', count(*) FILTER (WHERE status = 'Entregue'),
                    'Recusada', count(*) FILTER (WHERE status = 'Recusada'))
                FROM solicitacao WHERE colaborador_id = v_colaborador),
            'totalSolicitacoes', (SELECT count(*) FROM solicitacao WHERE colaborador_id = v_colaborador),
            'recentes', coalesce((
                SELECT jsonb_agg(app_privado.json_solicitacao(r.id) ORDER BY r.criada_em DESC)
                FROM (SELECT id, criada_em FROM solicitacao WHERE colaborador_id = v_colaborador
                      ORDER BY criada_em DESC LIMIT 3) r), '[]'::jsonb));
    END $$;

    -- Itens liberados para o cargo do colaborador (regra de elegibilidade, tabela cargo_item_permitido).
    CREATE OR REPLACE FUNCTION public.app_itens_elegiveis(p_token text) RETURNS jsonb
    LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, extensions, pg_temp AS $$
    DECLARE
        v_colaborador uuid := app_privado.colaborador_da_sessao(p_token);
    BEGIN
        RETURN coalesce((
            SELECT jsonb_agg(app_privado.json_item(i.id) ORDER BY cat.tipo = 'Epc', i.codigo)
            FROM colaborador c
            JOIN cargo_item_permitido p ON p.cargo_id = c.cargo_id
            JOIN item_epi_epc i ON i.id = p.item_id AND i.ativo
            JOIN categoria_item cat ON cat.id = i.categoria_id
            WHERE c.id = v_colaborador
        ), '[]'::jsonb);
    END $$;

    -- Motivos que o colaborador pode escolher ao pedir uma troca (Troca e Reposição).
    CREATE OR REPLACE FUNCTION public.app_motivos() RETURNS jsonb
    LANGUAGE sql SECURITY DEFINER SET search_path = public, extensions, pg_temp AS $$
        SELECT coalesce(jsonb_agg(app_privado.json_motivo(m.id)
                                  ORDER BY m.tipo_aplicavel = 'Reposicao', m.criado_em, m.descricao), '[]'::jsonb)
        FROM motivo_movimentacao m
        WHERE m.tipo_aplicavel IN ('Troca', 'Reposicao')
    $$;

    -- Mesmas validações de SolicitacaoService.CriarAsync (C#) e do apiEmMemoria.ts.
    CREATE OR REPLACE FUNCTION public.app_criar_solicitacao(p_token text, p_input jsonb) RETURNS jsonb
    LANGUAGE plpgsql SECURITY DEFINER SET search_path = public, extensions, pg_temp AS $$
    DECLARE
        v_colaborador colaborador;
        v_item item_epi_epc;
        v_motivo motivo_movimentacao;
        v_cargo text;
        v_quantidade int;
        v_tamanho text := nullif(trim(p_input ->> 'tamanho'), '');
        v_material jsonb := coalesce(p_input -> 'materialAntigo', '{}'::jsonb);
        v_relato text := trim(coalesce(v_material ->> 'relato', ''));
        v_assinatura text := coalesce(p_input ->> 'assinatura', '');
        v_em_andamento text;
        v_sequencial int;
        v_agora timestamp := app_privado.agora();
        v_id uuid := gen_random_uuid();
    BEGIN
        SELECT * INTO v_colaborador FROM colaborador WHERE id = app_privado.colaborador_da_sessao(p_token);

        BEGIN
            SELECT * INTO v_item FROM item_epi_epc WHERE id = (p_input ->> 'itemId')::uuid;
        EXCEPTION WHEN invalid_text_representation THEN
            v_item := NULL;
        END;
        IF v_item.id IS NULL THEN
            RAISE EXCEPTION 'Item não encontrado.';
        END IF;

        IF NOT EXISTS (SELECT 1 FROM cargo_item_permitido WHERE cargo_id = v_colaborador.cargo_id AND item_id = v_item.id) THEN
            SELECT nome INTO v_cargo FROM cargo WHERE id = v_colaborador.cargo_id;
            RAISE EXCEPTION 'O item "%" não é elegível para o cargo %.', v_item.nome, v_cargo;
        END IF;

        SELECT * INTO v_motivo FROM motivo_movimentacao
        WHERE codigo = p_input ->> 'motivoId' AND tipo_aplicavel IN ('Troca', 'Reposicao');
        IF v_motivo.id IS NULL THEN
            RAISE EXCEPTION 'Selecione o motivo da troca.';
        END IF;

        v_quantidade := CASE WHEN (p_input ->> 'quantidade') ~ '^\d{1,3}$' THEN (p_input ->> 'quantidade')::int END;
        IF v_quantidade IS NULL OR v_quantidade < 1 OR v_quantidade > 10 THEN
            RAISE EXCEPTION 'Quantidade inválida.';
        END IF;
        IF v_item.possui_tamanho AND v_tamanho IS NULL THEN
            RAISE EXCEPTION 'Informe o tamanho.';
        END IF;
        IF v_relato = '' THEN
            RAISE EXCEPTION 'Descreva o que aconteceu com o material antigo.';
        END IF;
        IF trim(v_assinatura) = '' THEN
            RAISE EXCEPTION 'A assinatura é obrigatória.';
        END IF;

        -- Um pedido em andamento por item; a trava serializa pedidos simultâneos (e a numeração).
        PERFORM pg_advisory_xact_lock(hashtext('gestao-epi:solicitacao'));
        SELECT protocolo INTO v_em_andamento FROM solicitacao
        WHERE colaborador_id = v_colaborador.id AND item_id = v_item.id AND status IN ('Pendente', 'EmAnalise', 'Aprovada')
        LIMIT 1;
        IF v_em_andamento IS NOT NULL THEN
            RAISE EXCEPTION 'Você já tem uma solicitação em andamento para este item (%).', v_em_andamento;
        END IF;

        SELECT greatest(coalesce(max(substring(protocolo FROM '(\d+)$')::int), 0), 2400) + 1 INTO v_sequencial FROM solicitacao;

        INSERT INTO solicitacao (
            id, protocolo, colaborador_id, item_id, motivo_id, tamanho, quantidade,
            material_data_entrega, material_fabricacao, material_marca, material_lote, relato,
            fotos, assinatura, status, criada_em, criado_em)
        VALUES (
            v_id,
            'SOL-' || extract(year FROM v_agora)::int || '-' || lpad(v_sequencial::text, 5, '0'),
            v_colaborador.id, v_item.id, v_motivo.id,
            CASE WHEN v_item.possui_tamanho THEN v_tamanho END,
            v_quantidade,
            -- Perda/extravio não tem material antigo para recolher.
            CASE WHEN NOT v_motivo.sem_devolucao AND nullif(v_material ->> 'dataEntrega', '') IS NOT NULL
                 THEN ((v_material ->> 'dataEntrega')::timestamptz AT TIME ZONE 'America/Bahia')::date END,
            CASE WHEN NOT v_motivo.sem_devolucao THEN left(nullif(trim(v_material ->> 'fabricacao'), ''), 7) END,
            CASE WHEN NOT v_motivo.sem_devolucao THEN left(nullif(trim(v_material ->> 'marca'), ''), 80) END,
            CASE WHEN NOT v_motivo.sem_devolucao THEN left(nullif(trim(v_material ->> 'lote'), ''), 40) END,
            left(v_relato, 1000),
            ARRAY(SELECT jsonb_array_elements_text(coalesce(p_input -> 'fotos', '[]'::jsonb))),
            v_assinatura, 'Pendente', v_agora, v_agora);

        INSERT INTO evento_solicitacao (id, solicitacao_id, status, data_hora, responsavel, comentario, criado_em)
        VALUES (gen_random_uuid(), v_id, 'Pendente', v_agora, v_colaborador.nome, 'Solicitação enviada pelo app.', v_agora);

        RETURN app_privado.json_solicitacao(v_id);
    END $$;

    -- ---- Permissões ----------------------------------------------------------------------------
    -- O Supabase concede EXECUTE em toda função nova de "public" aos papéis da API; restringimos
    -- explicitamente: auxiliares ninguém chama de fora, e as públicas só os papéis anon/authenticated.
    REVOKE ALL ON ALL FUNCTIONS IN SCHEMA app_privado FROM PUBLIC;
    REVOKE ALL ON FUNCTION public.app_login(text, text), public.app_logout(text), public.app_itens_em_posse(text),
        public.app_listar_solicitacoes(text, text, text), public.app_obter_solicitacao(text, uuid),
        public.app_resumo(text), public.app_itens_elegiveis(text), public.app_motivos(),
        public.app_criar_solicitacao(text, jsonb) FROM PUBLIC;

    DO $$
    DECLARE
        papel text;
    BEGIN
        FOREACH papel IN ARRAY ARRAY['anon', 'authenticated'] LOOP
            IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = papel) THEN
                EXECUTE format('REVOKE ALL ON ALL FUNCTIONS IN SCHEMA app_privado FROM %I', papel);
                EXECUTE format('GRANT EXECUTE ON FUNCTION public.app_login(text, text), public.app_logout(text), '
                    || 'public.app_itens_em_posse(text), public.app_listar_solicitacoes(text, text, text), '
                    || 'public.app_obter_solicitacao(text, uuid), public.app_resumo(text), public.app_itens_elegiveis(text), '
                    || 'public.app_motivos(), public.app_criar_solicitacao(text, jsonb) TO %I', papel);
            END IF;
        END LOOP;
    END $$;

    -- Pede ao PostgREST do Supabase para enxergar as funções novas imediatamente.
    NOTIFY pgrst, 'reload schema';

    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20260929231914_ApiMobile') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20260929231914_ApiMobile', '9.0.9');
    END IF;
END $EF$;
COMMIT;

