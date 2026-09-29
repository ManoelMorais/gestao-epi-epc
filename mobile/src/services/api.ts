import type {
  FiltroSolicitacoes,
  ItemDetalhado,
  ItemEmPosse,
  MotivoMovimentacao,
  NovaSolicitacaoInput,
  ResumoColaborador,
  Sessao,
  SolicitacaoDetalhada,
} from '@/types/dominio';

/**
 * Contrato que as telas usam para falar com o backend. Atendido por `apiSupabase`
 * (funções RPC no mesmo banco do desktop) ou, offline, por `apiEmMemoria`.
 *
 * Todo método recebe a sessão e trabalha só com os dados do colaborador logado:
 * o app é individual, ninguém vê ou cria solicitação em nome de outra pessoa.
 * (No backend real, o colaborador vem do token, não de um parâmetro.)
 */
export interface GestaoEpiApi {
  login(drt: string, senha: string): Promise<Sessao>;
  obterResumo(sessao: Sessao): Promise<ResumoColaborador>;
  listarItensEmPosse(sessao: Sessao): Promise<ItemEmPosse[]>;
  listarSolicitacoes(sessao: Sessao, filtro: FiltroSolicitacoes): Promise<SolicitacaoDetalhada[]>;
  obterSolicitacao(sessao: Sessao, id: string): Promise<SolicitacaoDetalhada | undefined>;
  listarItensElegiveis(sessao: Sessao): Promise<ItemDetalhado[]>;
  listarMotivos(): Promise<MotivoMovimentacao[]>;
  criarSolicitacao(sessao: Sessao, input: NovaSolicitacaoInput): Promise<SolicitacaoDetalhada>;
}

export { ErroNegocio } from './erros';
// Backend real: o mesmo banco (Supabase) do desktop. Para demonstrar sem internet, troque por
// `export { apiEmMemoria as api } from './apiEmMemoria';` (dados de exemplo locais).
export { apiSupabase as api } from './apiSupabase';
