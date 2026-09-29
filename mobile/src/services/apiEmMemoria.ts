import {
  cargos,
  categorias,
  colaboradores,
  gerarEntregas,
  gerarSolicitacoes,
  itens,
  itensPermitidosPorCargo,
  motivos,
  motivosDeSolicitacao,
  proximoProtocolo,
  SENHA_DEMO,
  unidades,
} from '@/data/seed';
import type {
  ColaboradorDetalhado,
  Entrega,
  ItemDetalhado,
  ItemEmPosse,
  ResumoColaborador,
  Sessao,
  Solicitacao,
  SolicitacaoDetalhada,
  StatusSolicitacao,
} from '@/types/dominio';
import { inicioDoDia, normalizarTexto } from '@/utils/formatacao';
import type { GestaoEpiApi } from './api';
import { ErroNegocio } from './erros';

// Implementação em memória do contrato da API — mesmo papel da camada
// Infrastructure/InMemory do desktop. Simula latência de rede para as telas
// exercitarem os estados de carregamento como fariam com o backend real.

const entregas: Entrega[] = gerarEntregas();
const solicitacoes: Solicitacao[] = gerarSolicitacoes();

const latencia = (ms = 350) => new Promise((resolve) => setTimeout(resolve, ms));

function detalharColaborador(id: string): ColaboradorDetalhado {
  const colaborador = colaboradores.find((c) => c.id === id)!;
  return {
    ...colaborador,
    cargo: cargos.find((c) => c.id === colaborador.cargoId)!,
    unidade: unidades.find((u) => u.id === colaborador.unidadeId)!,
  };
}

function detalharItem(id: string): ItemDetalhado {
  const item = itens.find((i) => i.id === id)!;
  return { ...item, categoria: categorias.find((c) => c.id === item.categoriaId)! };
}

function detalharSolicitacao(s: Solicitacao): SolicitacaoDetalhada {
  const { itemId, motivoId, ...resto } = s;
  return { ...resto, item: detalharItem(itemId), motivo: motivos.find((m) => m.id === motivoId)! };
}

/** Solicitações do colaborador da sessão — o único recorte que o app enxerga. */
const doColaborador = (sessao: Sessao) => solicitacoes.filter((s) => s.colaboradorId === sessao.colaborador.id);

function calcularItensEmPosse(colaboradorId: string): ItemEmPosse[] {
  // A última movimentação confirmada de cada item define o que está com o colaborador.
  const ultimaPorItem = new Map<string, { entrega: Entrega; quantidade: number; tamanho?: string }>();
  entregas
    .filter((e) => e.colaboradorId === colaboradorId && e.status === 'Confirmada')
    .sort((a, b) => a.dataHora.localeCompare(b.dataHora))
    .forEach((e) => e.itens.forEach((i) => ultimaPorItem.set(i.itemId, { entrega: e, quantidade: i.quantidade, tamanho: i.tamanho })));

  const hoje = inicioDoDia(new Date()).getTime();
  return [...ultimaPorItem.entries()]
    .filter(([, u]) => u.entrega.tipoMovimentacao !== 'Devolucao')
    .map(([itemId, u]) => {
      const item = detalharItem(itemId);
      const posse: ItemEmPosse = { item, quantidade: u.quantidade, tamanho: u.tamanho, recebidoEm: u.entrega.dataHora };
      if (item.validadePadraoMeses) {
        const vence = new Date(u.entrega.dataHora);
        vence.setMonth(vence.getMonth() + item.validadePadraoMeses);
        posse.venceEm = vence.toISOString();
        posse.diasParaVencer = Math.round((inicioDoDia(vence).getTime() - hoje) / 86_400_000);
      }
      return posse;
    })
    // Vencidos e próximos de vencer primeiro; itens sem validade no fim.
    .sort((a, b) => (a.diasParaVencer ?? Infinity) - (b.diasParaVencer ?? Infinity));
}

export const apiEmMemoria: GestaoEpiApi = {
  async login(drt, senha) {
    await latencia(700);
    const colaborador = colaboradores.find((c) => c.drt === drt.trim());
    if (!colaborador || senha !== SENHA_DEMO) throw new ErroNegocio('DRT ou senha inválidos.');
    if (colaborador.status === 'Inativo') {
      throw new ErroNegocio('Seu acesso está desativado. Procure o RH ou o almoxarifado da sua unidade.');
    }
    return { colaborador: detalharColaborador(colaborador.id) };
  },

  async obterResumo(sessao) {
    await latencia();
    const minhas = doColaborador(sessao);
    const porStatus: Record<StatusSolicitacao, number> = { Pendente: 0, EmAnalise: 0, Aprovada: 0, Entregue: 0, Recusada: 0 };
    minhas.forEach((s) => (porStatus[s.status] += 1));
    const resumo: ResumoColaborador = {
      itensEmPosse: calcularItensEmPosse(sessao.colaborador.id),
      porStatus,
      totalSolicitacoes: minhas.length,
      recentes: minhas.slice(0, 3).map(detalharSolicitacao),
    };
    return resumo;
  },

  async listarItensEmPosse(sessao) {
    await latencia(250);
    return calcularItensEmPosse(sessao.colaborador.id);
  },

  async listarSolicitacoes(sessao, filtro) {
    await latencia();
    const termo = normalizarTexto(filtro.termo ?? '');
    return doColaborador(sessao)
      .filter((s) => {
        if (!filtro.grupo) return true;
        if (filtro.grupo === 'andamento') return s.status === 'Pendente' || s.status === 'EmAnalise';
        return s.status === filtro.grupo;
      })
      .map(detalharSolicitacao)
      .filter((s) => !termo || normalizarTexto(s.item.nome).includes(termo) || normalizarTexto(s.protocolo).includes(termo));
  },

  async obterSolicitacao(sessao, id) {
    await latencia(200);
    // Mesmo sabendo o id, ninguém abre a solicitação de outra pessoa.
    const s = doColaborador(sessao).find((x) => x.id === id);
    return s && detalharSolicitacao(s);
  },

  async listarItensElegiveis(sessao) {
    await latencia(300);
    return (itensPermitidosPorCargo[sessao.colaborador.cargoId] ?? []).map(detalharItem);
  },

  async listarMotivos() {
    return motivosDeSolicitacao;
  },

  async criarSolicitacao(sessao, input) {
    await latencia(900);
    const colaborador = colaboradores.find((c) => c.id === sessao.colaborador.id);
    if (!colaborador || colaborador.status === 'Inativo') throw new ErroNegocio('Seu acesso está desativado.');

    // Mesma regra do EntregaService no backend: só itens liberados para o cargo.
    if (!(itensPermitidosPorCargo[colaborador.cargoId] ?? []).includes(input.itemId)) {
      const cargo = cargos.find((c) => c.id === colaborador.cargoId)!;
      throw new ErroNegocio(`O item "${detalharItem(input.itemId).nome}" não é elegível para o cargo ${cargo.nome}.`);
    }
    const motivo = motivosDeSolicitacao.find((m) => m.id === input.motivoId);
    if (!motivo) throw new ErroNegocio('Selecione o motivo da troca.');
    if (input.quantidade < 1 || input.quantidade > 10) throw new ErroNegocio('Quantidade inválida.');
    if (!input.materialAntigo.relato.trim()) throw new ErroNegocio('Descreva o que aconteceu com o material antigo.');
    if (!input.assinatura) throw new ErroNegocio('A assinatura é obrigatória.');

    // Evita pedidos duplicados do mesmo item enquanto um anterior ainda está em andamento.
    const emAndamento = doColaborador(sessao).find((s) => s.itemId === input.itemId && ['Pendente', 'EmAnalise', 'Aprovada'].includes(s.status));
    if (emAndamento) {
      throw new ErroNegocio(`Você já tem uma solicitação em andamento para este item (${emAndamento.protocolo}).`);
    }

    const agora = new Date();
    const nova: Solicitacao = {
      id: `sol-${Date.now()}`,
      protocolo: proximoProtocolo(agora),
      colaboradorId: colaborador.id,
      itemId: input.itemId,
      tamanho: input.tamanho,
      quantidade: input.quantidade,
      motivoId: input.motivoId,
      materialAntigo: motivo.semDevolucao ? { relato: input.materialAntigo.relato.trim() } : { ...input.materialAntigo, relato: input.materialAntigo.relato.trim() },
      fotos: input.fotos,
      assinatura: input.assinatura,
      status: 'Pendente',
      criadaEm: agora.toISOString(),
      historico: [{ status: 'Pendente', dataHora: agora.toISOString(), responsavel: 'Você', comentario: 'Solicitação enviada pelo app.' }],
    };
    solicitacoes.unshift(nova);
    return detalharSolicitacao(nova);
  },
};
