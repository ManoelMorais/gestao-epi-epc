import type { ComponentProps } from 'react';
import type MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import type { ItemEmPosse, StatusColaborador, StatusSolicitacao } from '@/types/dominio';

// Mesma paleta da marca usada no desktop (Themes/Colors.xaml), para os dois
// clientes parecerem um único produto.
export const cores = {
  ink: '#0F1720',
  inkAlt: '#161F2C',
  inkSuave: '#4B5566',
  inkMudo: '#8B93A3',
  paper: '#F3F4F8',
  surface: '#FFFFFF',
  linha: '#E5E7EE',
  azul: '#1D4ED8',
  azulEscuro: '#1638A6',
  azulSuave: '#EAF0FE',
  laranja: '#E85D19',
  laranjaSuave: '#FDECE1',
  verde: '#1E824C',
  verdeSuave: '#E7F5EC',
  vermelho: '#C4321A',
  vermelhoSuave: '#FBE7E2',
  roxo: '#6D28D9',
  roxoSuave: '#F1E8FD',
  ciano: '#0E7490',
  cianoSuave: '#E1F3F7',
} as const;

export const gradienteMarca = [cores.azul, cores.azulEscuro] as const;

export const raio = { p: 8, m: 14, g: 20, pill: 999 } as const;
export const espaco = { xs: 4, s: 8, m: 12, l: 16, xl: 20, xxl: 28 } as const;

export const sombra = {
  shadowColor: '#0F1720',
  shadowOpacity: 0.07,
  shadowRadius: 14,
  shadowOffset: { width: 0, height: 6 },
  elevation: 3,
} as const;

export type NomeIcone = ComponentProps<typeof MaterialCommunityIcons>['name'];

export interface Visual {
  rotulo: string;
  cor: string;
  fundo: string;
  icone: NomeIcone;
}

export const visualStatusSolicitacao: Record<StatusSolicitacao, Visual & { descricao: string }> = {
  Pendente: { rotulo: 'Enviada', cor: cores.inkSuave, fundo: cores.linha, icone: 'send-clock-outline', descricao: 'Aguardando análise do almoxarifado' },
  EmAnalise: { rotulo: 'Em análise', cor: cores.roxo, fundo: cores.roxoSuave, icone: 'magnify', descricao: 'Segurança do trabalho avaliando o pedido' },
  Aprovada: { rotulo: 'Aprovada', cor: cores.azul, fundo: cores.azulSuave, icone: 'check-decagram', descricao: 'Liberada — retire no almoxarifado' },
  Entregue: { rotulo: 'Entregue', cor: cores.verde, fundo: cores.verdeSuave, icone: 'package-variant-closed-check', descricao: 'Item novo entregue' },
  Recusada: { rotulo: 'Recusada', cor: cores.vermelho, fundo: cores.vermelhoSuave, icone: 'close-circle-outline', descricao: 'Pedido não aprovado' },
};

/** Ordem do fluxo, usada na linha do tempo do detalhe. */
export const etapasSolicitacao: StatusSolicitacao[] = ['Pendente', 'EmAnalise', 'Aprovada', 'Entregue'];

export const visualStatusColaborador: Record<StatusColaborador, Visual> = {
  Ativo: { rotulo: 'Ativo', cor: cores.verde, fundo: cores.verdeSuave, icone: 'account-check' },
  Afastado: { rotulo: 'Afastado', cor: cores.laranja, fundo: cores.laranjaSuave, icone: 'account-clock' },
  Inativo: { rotulo: 'Inativo', cor: cores.inkSuave, fundo: cores.linha, icone: 'account-off' },
};

/** Janela em que um EPI passa a ser mostrado como "vence em breve". */
export const DIAS_ALERTA_VALIDADE = 45;

export function visualValidade(posse: ItemEmPosse): Visual {
  const dias = posse.diasParaVencer;
  if (dias === undefined) return { rotulo: 'Sem validade', cor: cores.inkSuave, fundo: cores.linha, icone: 'infinity' };
  if (dias < 0) return { rotulo: `Vencido há ${-dias} ${-dias === 1 ? 'dia' : 'dias'}`, cor: cores.vermelho, fundo: cores.vermelhoSuave, icone: 'alert-octagon' };
  if (dias === 0) return { rotulo: 'Vence hoje', cor: cores.vermelho, fundo: cores.vermelhoSuave, icone: 'alert-octagon' };
  if (dias <= DIAS_ALERTA_VALIDADE) return { rotulo: `Vence em ${dias} ${dias === 1 ? 'dia' : 'dias'}`, cor: cores.laranja, fundo: cores.laranjaSuave, icone: 'clock-alert-outline' };
  return { rotulo: 'Em dia', cor: cores.verde, fundo: cores.verdeSuave, icone: 'shield-check' };
}

/** Ícone por categoria do catálogo, para as listas de EPI. */
export function iconeCategoria(categoriaId: string): NomeIcone {
  switch (categoriaId) {
    case 'cat-cabeca':
      return 'hard-hat';
    case 'cat-maos':
      return 'hand-back-right-outline';
    case 'cat-quedas':
      return 'carabiner';
    case 'cat-vestimenta':
      return 'tshirt-crew-outline';
    case 'cat-auditiva':
      return 'ear-hearing';
    case 'cat-pes':
      return 'shoe-print';
    default:
      return 'traffic-cone';
  }
}
