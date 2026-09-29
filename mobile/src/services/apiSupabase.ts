import type {
  ItemDetalhado,
  ItemEmPosse,
  MotivoMovimentacao,
  ResumoColaborador,
  Sessao,
  SolicitacaoDetalhada,
} from '@/types/dominio';
import type { GestaoEpiApi } from './api';
import { SUPABASE_KEY, SUPABASE_URL } from './configSupabase';
import { ErroNegocio } from './erros';

// Implementação real do contrato da API: o mesmo banco do desktop (Supabase/PostgreSQL).
// Cada método chama uma função RPC do banco (ver database/README.md e
// src/GestaoEpiEpc.Infrastructure/Persistence/Sql/ApiMobile.sql no projeto do desktop), que já
// devolve o JSON no formato dos tipos de '@/types/dominio'. As regras de negócio (elegibilidade
// por cargo, uma solicitação em andamento por item, validações) rodam no banco — o app só exibe.

const TEMPO_LIMITE_MS = 20_000;

/** Erro "de negócio" levantado pelas funções do banco (RAISE EXCEPTION → código P0001). */
const CODIGO_ERRO_NEGOCIO = 'P0001';

async function rpc<T>(funcao: string, parametros: Record<string, unknown> = {}): Promise<T> {
  const controle = new AbortController();
  const limite = setTimeout(() => controle.abort(), TEMPO_LIMITE_MS);

  let resposta: Response;
  try {
    resposta = await fetch(`${SUPABASE_URL}/rest/v1/rpc/${funcao}`, {
      method: 'POST',
      headers: {
        apikey: SUPABASE_KEY,
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      body: JSON.stringify(parametros),
      signal: controle.signal,
    });
  } catch {
    throw new ErroNegocio('Não foi possível conectar ao servidor. Verifique sua conexão com a internet e tente de novo.');
  } finally {
    clearTimeout(limite);
  }

  const corpo = await resposta.text();
  const dados = corpo ? JSON.parse(corpo) : null;

  if (!resposta.ok) {
    const mensagem: string | undefined = dados?.message;
    if (dados?.code === CODIGO_ERRO_NEGOCIO && mensagem) throw new ErroNegocio(mensagem);
    throw new Error(mensagem ?? `Falha ao chamar ${funcao} (HTTP ${resposta.status}).`);
  }
  return dados as T;
}

function token(sessao: Sessao): string {
  if (!sessao.token) throw new ErroNegocio('Sua sessão expirou. Entre novamente.');
  return sessao.token;
}

export const apiSupabase: GestaoEpiApi = {
  async login(drt, senha) {
    const resposta = await rpc<Sessao & { token: string }>('app_login', { p_drt: drt.trim(), p_senha: senha });
    return { colaborador: resposta.colaborador, token: resposta.token };
  },

  async obterResumo(sessao) {
    return rpc<ResumoColaborador>('app_resumo', { p_token: token(sessao) });
  },

  async listarItensEmPosse(sessao) {
    return rpc<ItemEmPosse[]>('app_itens_em_posse', { p_token: token(sessao) });
  },

  async listarSolicitacoes(sessao, filtro) {
    return rpc<SolicitacaoDetalhada[]>('app_listar_solicitacoes', {
      p_token: token(sessao),
      p_grupo: filtro.grupo ?? null,
      p_termo: filtro.termo?.trim() || null,
    });
  },

  async obterSolicitacao(sessao, id) {
    // O banco devolve null para solicitação inexistente ou de outro colaborador.
    const solicitacao = await rpc<SolicitacaoDetalhada | null>('app_obter_solicitacao', { p_token: token(sessao), p_id: id });
    return solicitacao ?? undefined;
  },

  async listarItensElegiveis(sessao) {
    return rpc<ItemDetalhado[]>('app_itens_elegiveis', { p_token: token(sessao) });
  },

  async listarMotivos() {
    return rpc<MotivoMovimentacao[]>('app_motivos');
  },

  async criarSolicitacao(sessao, input) {
    return rpc<SolicitacaoDetalhada>('app_criar_solicitacao', { p_token: token(sessao), p_input: input });
  },
};
