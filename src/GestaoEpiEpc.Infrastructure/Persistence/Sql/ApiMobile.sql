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
