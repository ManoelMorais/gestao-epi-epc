// Espelho das entidades de GestaoEpiEpc.Domain. Os nomes e enums seguem o backend em C#
// para que, quando a API existir, os contratos JSON encaixem sem tradução.

export type TipoItem = 'Epi' | 'Epc';
export type TipoMovimentacao = 'EntregaInicial' | 'Reposicao' | 'Troca' | 'Devolucao';
export type StatusEntrega = 'Confirmada' | 'Estornada';
export type StatusColaborador = 'Ativo' | 'Inativo' | 'Afastado';

/**
 * Ciclo de vida de uma solicitação feita pelo colaborador no app:
 * Pendente (enviada) → EmAnalise (almoxarifado/SST avaliando) → Aprovada (liberada para
 * retirada) → Entregue (vira uma Entrega no sistema). Pode ser Recusada na análise.
 */
export type StatusSolicitacao = 'Pendente' | 'EmAnalise' | 'Aprovada' | 'Entregue' | 'Recusada';

export interface Unidade {
  id: string;
  nome: string;
  sigla: string;
  cidade: string;
}

export interface Cargo {
  id: string;
  nome: string;
}

export interface CategoriaItem {
  id: string;
  nome: string;
  tipo: TipoItem;
}

export interface ItemEpiEpc {
  id: string;
  codigo: string;
  nome: string;
  categoriaId: string;
  numeroCa: string | null;
  validadePadraoMeses: number | null;
  possuiTamanho: boolean;
}

export interface Colaborador {
  id: string;
  /** DRT — número de registro do colaborador (a "matrícula" do desktop). */
  drt: string;
  nome: string;
  cargoId: string;
  area: string;
  unidadeId: string;
  status: StatusColaborador;
  admissao: string; // ISO
  email: string;
  telefone: string;
}

export interface MotivoMovimentacao {
  id: string;
  descricao: string;
  tipoAplicavel: TipoMovimentacao;
  /** Sem material antigo para devolver (ex.: perda) — o formulário não pede lote/fabricação/marca. */
  semDevolucao?: boolean;
}

export interface EntregaItem {
  itemId: string;
  quantidade: number;
  tamanho?: string;
}

/** Movimentação já efetivada pelo almoxarifado (histórico do colaborador). */
export interface Entrega {
  id: string;
  colaboradorId: string;
  motivoId: string;
  tipoMovimentacao: TipoMovimentacao;
  status: StatusEntrega;
  dataHora: string; // ISO
  itens: EntregaItem[];
}

export interface EventoSolicitacao {
  status: StatusSolicitacao;
  dataHora: string; // ISO
  responsavel: string;
  comentario?: string;
}

/** Informações sobre o material que está sendo substituído (bloco "material antigo" do SIGME). */
export interface MaterialAntigo {
  dataEntrega?: string; // ISO (dia)
  fabricacao?: string; // "MM/AAAA"
  marca?: string;
  lote?: string;
  relato: string;
}

/** Pedido de troca feito pelo próprio colaborador no app. */
export interface Solicitacao {
  id: string;
  protocolo: string;
  colaboradorId: string;
  itemId: string;
  tamanho?: string;
  quantidade: number;
  motivoId: string;
  materialAntigo: MaterialAntigo;
  fotos: string[]; // URIs locais (no backend real, URLs após upload)
  assinatura: string; // caminhos SVG da assinatura desenhada no app
  status: StatusSolicitacao;
  criadaEm: string; // ISO
  historico: EventoSolicitacao[];
}

// ---- Modelos de leitura (o que as telas consomem) --------------------------

export interface ColaboradorDetalhado extends Colaborador {
  cargo: Cargo;
  unidade: Unidade;
}

export interface Sessao {
  colaborador: ColaboradorDetalhado;
  /** Token de sessão emitido pelo backend no login (ausente na implementação em memória). */
  token?: string;
}

export interface ItemDetalhado extends ItemEpiEpc {
  categoria: CategoriaItem;
}

export interface SolicitacaoDetalhada extends Omit<Solicitacao, 'itemId' | 'motivoId'> {
  item: ItemDetalhado;
  motivo: MotivoMovimentacao;
}

/** Um EPI/EPC que está com o colaborador, com a validade calculada a partir da última entrega. */
export interface ItemEmPosse {
  item: ItemDetalhado;
  quantidade: number;
  tamanho?: string;
  recebidoEm: string; // ISO
  venceEm?: string; // ISO — ausente para itens sem validade (ex.: EPC)
  diasParaVencer?: number;
}

export interface ResumoColaborador {
  itensEmPosse: ItemEmPosse[];
  porStatus: Record<StatusSolicitacao, number>;
  totalSolicitacoes: number;
  recentes: SolicitacaoDetalhada[];
}

/** "andamento" agrupa as que ainda esperam resposta (enviada + em análise). */
export type GrupoStatus = 'andamento' | 'Aprovada' | 'Entregue' | 'Recusada';

export interface FiltroSolicitacoes {
  grupo?: GrupoStatus;
  termo?: string;
}

export interface NovaSolicitacaoInput {
  itemId: string;
  tamanho?: string;
  quantidade: number;
  motivoId: string;
  materialAntigo: MaterialAntigo;
  fotos: string[];
  assinatura: string;
}
