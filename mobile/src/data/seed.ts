import type {
  Cargo,
  CategoriaItem,
  Colaborador,
  Entrega,
  EventoSolicitacao,
  ItemEpiEpc,
  MotivoMovimentacao,
  Solicitacao,
  StatusSolicitacao,
  TipoMovimentacao,
  Unidade,
} from '@/types/dominio';

// Dados de exemplo equivalentes aos do DataSeeder.cs do desktop, para o app ser
// demonstrável antes de a API existir. Nada aqui sobrevive a um reinício do app.

// ---- Unidades ------------------------------------------------------------
export const unidades: Unidade[] = [
  { id: 'uni-aps', nome: 'Amperion Sul', sigla: 'APS', cidade: 'Porto Sereno' },
  { id: 'uni-apn', nome: 'Amperion Norte', sigla: 'APN', cidade: 'Vale do Norte' },
];

// ---- Cargos --------------------------------------------------------------
export const cargos: Cargo[] = [
  { id: 'car-eletricista', nome: 'Eletricista de Rede' },
  { id: 'car-auxiliar', nome: 'Auxiliar de Almoxarifado' },
  { id: 'car-sst', nome: 'Técnico de Segurança do Trabalho' },
  { id: 'car-adm', nome: 'Analista Administrativo' },
  { id: 'car-supervisor', nome: 'Supervisor de Campo' },
  { id: 'car-manutencao', nome: 'Técnico de Manutenção' },
];

// ---- Categorias e catálogo -------------------------------------------------
export const categorias: CategoriaItem[] = [
  { id: 'cat-cabeca', nome: 'Proteção da Cabeça', tipo: 'Epi' },
  { id: 'cat-maos', nome: 'Proteção das Mãos', tipo: 'Epi' },
  { id: 'cat-quedas', nome: 'Proteção contra Quedas', tipo: 'Epi' },
  { id: 'cat-vestimenta', nome: 'Vestimenta', tipo: 'Epi' },
  { id: 'cat-auditiva', nome: 'Proteção Auditiva', tipo: 'Epi' },
  { id: 'cat-pes', nome: 'Proteção dos Pés', tipo: 'Epi' },
  { id: 'cat-sinalizacao', nome: 'Sinalização e Isolamento', tipo: 'Epc' },
];

export const itens: ItemEpiEpc[] = [
  { id: 'itm-capacete', codigo: 'EPI-001', nome: 'Capacete de Segurança Classe B', categoriaId: 'cat-cabeca', numeroCa: '31469', validadePadraoMeses: 60, possuiTamanho: false },
  { id: 'itm-oculos', codigo: 'EPI-002', nome: 'Óculos de Proteção Incolor', categoriaId: 'cat-cabeca', numeroCa: '25763', validadePadraoMeses: 24, possuiTamanho: false },
  { id: 'itm-luva-isolante', codigo: 'EPI-003', nome: 'Luva Isolante de Borracha Classe 2', categoriaId: 'cat-maos', numeroCa: '28871', validadePadraoMeses: 12, possuiTamanho: true },
  { id: 'itm-cinto', codigo: 'EPI-004', nome: 'Cinto de Segurança Tipo Paraquedista', categoriaId: 'cat-quedas', numeroCa: '34115', validadePadraoMeses: 24, possuiTamanho: true },
  { id: 'itm-nomex', codigo: 'EPI-005', nome: 'Uniforme NOMEX Manga Longa', categoriaId: 'cat-vestimenta', numeroCa: '30044', validadePadraoMeses: 24, possuiTamanho: true },
  { id: 'itm-luva-raspa', codigo: 'EPI-006', nome: 'Luva de Raspa Reforçada', categoriaId: 'cat-maos', numeroCa: '19207', validadePadraoMeses: 6, possuiTamanho: true },
  { id: 'itm-talabarte', codigo: 'EPI-007', nome: 'Talabarte Duplo em Y', categoriaId: 'cat-quedas', numeroCa: '37788', validadePadraoMeses: 24, possuiTamanho: false },
  { id: 'itm-auricular', codigo: 'EPI-008', nome: 'Protetor Auricular Tipo Plug', categoriaId: 'cat-auditiva', numeroCa: '5745', validadePadraoMeses: 6, possuiTamanho: false },
  { id: 'itm-bota', codigo: 'EPI-009', nome: 'Bota de Segurança com Bico de Aço', categoriaId: 'cat-pes', numeroCa: '40311', validadePadraoMeses: 12, possuiTamanho: true },
  { id: 'itm-fita', codigo: 'EPC-001', nome: 'Fita Zebrada de Sinalização', categoriaId: 'cat-sinalizacao', numeroCa: null, validadePadraoMeses: null, possuiTamanho: false },
  { id: 'itm-cone', codigo: 'EPC-002', nome: 'Cone de Sinalização', categoriaId: 'cat-sinalizacao', numeroCa: null, validadePadraoMeses: null, possuiTamanho: false },
  { id: 'itm-sinalizador', codigo: 'EPC-003', nome: 'Sinalizador Luminoso Portátil', categoriaId: 'cat-sinalizacao', numeroCa: null, validadePadraoMeses: null, possuiTamanho: false },
];

// ---- Elegibilidade por cargo (CargoItemPermitido) ----------------------------
export const itensPermitidosPorCargo: Record<string, string[]> = {
  'car-eletricista': ['itm-capacete', 'itm-oculos', 'itm-luva-isolante', 'itm-cinto', 'itm-nomex', 'itm-talabarte', 'itm-bota', 'itm-fita', 'itm-cone'],
  'car-auxiliar': ['itm-capacete', 'itm-oculos', 'itm-luva-raspa', 'itm-bota'],
  'car-sst': ['itm-capacete', 'itm-oculos', 'itm-auricular', 'itm-fita', 'itm-cone', 'itm-sinalizador'],
  // Função de escritório: nenhum EPI/EPC de campo é elegível.
  'car-adm': [],
  'car-supervisor': ['itm-capacete', 'itm-oculos', 'itm-luva-isolante', 'itm-nomex', 'itm-bota', 'itm-cone'],
  'car-manutencao': ['itm-capacete', 'itm-oculos', 'itm-luva-raspa', 'itm-auricular', 'itm-cinto', 'itm-talabarte', 'itm-bota'],
};

// ---- Motivos -------------------------------------------------------------
export const motivos: MotivoMovimentacao[] = [
  { id: 'mot-novo', descricao: 'Novo colaborador', tipoAplicavel: 'EntregaInicial' },
  { id: 'mot-desgaste', descricao: 'Desgaste natural', tipoAplicavel: 'Troca' },
  { id: 'mot-avaria', descricao: 'Dano em serviço', tipoAplicavel: 'Troca' },
  { id: 'mot-validade', descricao: 'Validade vencida', tipoAplicavel: 'Troca' },
  { id: 'mot-tamanho', descricao: 'Tamanho inadequado', tipoAplicavel: 'Troca' },
  { id: 'mot-defeito', descricao: 'Defeito de fabricação', tipoAplicavel: 'Troca' },
  { id: 'mot-perda', descricao: 'Perda ou extravio', tipoAplicavel: 'Reposicao', semDevolucao: true },
  { id: 'mot-desligamento', descricao: 'Devolução por desligamento', tipoAplicavel: 'Devolucao' },
];

/** Motivos que o colaborador pode escolher ao pedir uma troca no app. */
export const motivosDeSolicitacao = motivos.filter((m) => m.tipoAplicavel === 'Troca' || m.tipoAplicavel === 'Reposicao');

// ---- Colaboradores (cada um tem o próprio acesso ao app) ---------------------
const c = (
  id: string,
  drt: string,
  nome: string,
  cargoId: string,
  area: string,
  unidadeId: string,
  admissao: string,
  telefone: string,
  status: Colaborador['status'] = 'Ativo',
): Colaborador => {
  const [primeiro, ...resto] = nome.toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, '').split(' ');
  return { id, drt, nome, cargoId, area, unidadeId, admissao, telefone, status, email: `${primeiro}.${resto[resto.length - 1]}@amperion.com.br` };
};

export const colaboradores: Colaborador[] = [
  c('col-roberto', '10234', 'Roberto Carlos Nascimento', 'car-eletricista', 'Manutenção de Rede', 'uni-aps', '2019-03-11', '(79) 99812-4410'),
  c('col-juliana', '10567', 'Juliana Alves Pereira', 'car-eletricista', 'Manutenção de Rede', 'uni-aps', '2020-07-01', '(79) 99734-2281'),
  c('col-marcos', '10890', 'Marcos Vinícius Teixeira', 'car-auxiliar', 'Almoxarifado', 'uni-aps', '2021-02-15', '(79) 99655-0192'),
  c('col-patricia', '11023', 'Patrícia Gomes Ferreira', 'car-sst', 'Segurança do Trabalho', 'uni-aps', '2018-09-03', '(79) 99901-7763'),
  c('col-rafael', '11345', 'Rafael Costa Andrade', 'car-adm', 'Administrativo', 'uni-aps', '2022-01-10', '(79) 99588-3304'),
  c('col-camila', '11789', 'Camila Souza Ribeiro', 'car-eletricista', 'Manutenção de Rede', 'uni-apn', '2021-05-24', '(65) 99672-1185'),
  c('col-fernando', '12012', 'Fernando Henrique Lopes', 'car-supervisor', 'Manutenção de Rede', 'uni-aps', '2016-11-07', '(79) 99843-5520'),
  c('col-bianca', '12045', 'Bianca Oliveira Cardoso', 'car-manutencao', 'Manutenção de Rede', 'uni-aps', '2023-04-17', '(79) 99710-6648'),
  c('col-diego', '12078', 'Diego Martins Rocha', 'car-eletricista', 'Manutenção de Rede', 'uni-aps', '2019-08-19', '(79) 99627-9031', 'Afastado'),
  c('col-larissa', '12101', 'Larissa Fernandes Melo', 'car-auxiliar', 'Almoxarifado', 'uni-aps', '2020-10-05', '(79) 99555-4417', 'Inativo'),
  c('col-eduardo', '12134', 'Eduardo Pereira Gomes', 'car-manutencao', 'Manutenção de Rede', 'uni-aps', '2022-06-13', '(79) 99788-2256'),
  c('col-vanessa', '12167', 'Vanessa Almeida Torres', 'car-adm', 'Administrativo', 'uni-aps', '2023-09-01', '(79) 99604-8872'),
  c('col-gustavo', '12190', 'Gustavo Henrique Silva', 'car-eletricista', 'Manutenção de Rede', 'uni-apn', '2020-02-03', '(65) 99745-3319'),
  c('col-renata', '12223', 'Renata Cristina Barbosa', 'car-supervisor', 'Manutenção de Rede', 'uni-apn', '2017-12-11', '(65) 99831-0074'),
  c('col-thiago', '12256', 'Thiago Souza Martins', 'car-sst', 'Segurança do Trabalho', 'uni-apn', '2021-08-23', '(65) 99690-5528'),
  c('col-amanda', '12289', 'Amanda Ribeiro Costa', 'car-auxiliar', 'Almoxarifado', 'uni-apn', '2024-01-08', '(65) 99577-1143'),
  c('col-bruno', '12312', 'Bruno César Andrade', 'car-manutencao', 'Manutenção de Rede', 'uni-apn', '2022-10-31', '(65) 99802-6690'),
  c('col-isabela', '12345', 'Isabela Nunes Carvalho', 'car-eletricista', 'Manutenção de Rede', 'uni-apn', '2023-03-20', '(65) 99718-4402'),
];

/** Senha de demonstração, igual para todos. Quando houver API, a autenticação vai para o backend. */
export const SENHA_DEMO = '123456';

// ---- Utilitários de geração ---------------------------------------------------

/** Gerador pseudoaleatório com semente fixa (mulberry32): o mesmo histórico em toda execução. */
function criarAleatorio(semente: number) {
  let estado = semente;
  const proximo = () => {
    estado = (estado + 0x6d2b79f5) | 0;
    let t = Math.imul(estado ^ (estado >>> 15), 1 | estado);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
  return {
    numero: proximo,
    inteiro: (min: number, maxExclusivo: number) => min + Math.floor(proximo() * (maxExclusivo - min)),
    escolher: <T,>(lista: readonly T[]) => lista[Math.floor(proximo() * lista.length)],
  };
}

function diasAtras(dias: number, horas = 0, minutos = 0) {
  const d = new Date();
  d.setDate(d.getDate() - dias);
  d.setHours(d.getHours() - horas, d.getMinutes() - minutos, 0, 0);
  return d;
}

let sequencialProtocolo = 2400;
export function proximoProtocolo(data = new Date()) {
  sequencialProtocolo += 1;
  return `SOL-${data.getFullYear()}-${String(sequencialProtocolo).padStart(5, '0')}`;
}

/** Grade de tamanhos exibida no formulário, conforme a categoria do item. */
export function tamanhosSugeridos(item: ItemEpiEpc): string[] {
  switch (item.categoriaId) {
    case 'cat-pes':
      return ['36', '37', '38', '39', '40', '41', '42', '43', '44'];
    case 'cat-maos':
      return ['7', '8', '9', '10', '11'];
    default:
      return ['PP', 'P', 'M', 'G', 'GG'];
  }
}

// Tamanho "de cadastro" de cada colaborador, usado para gerar histórico coerente.
const TAMANHOS: Record<string, { roupa: string; luva: string; bota: string }> = {
  'col-roberto': { roupa: 'G', luva: '10', bota: '42' },
  'col-juliana': { roupa: 'M', luva: '8', bota: '37' },
  'col-marcos': { roupa: 'G', luva: '9', bota: '41' },
  'col-patricia': { roupa: 'P', luva: '7', bota: '36' },
  'col-camila': { roupa: 'M', luva: '8', bota: '38' },
  'col-fernando': { roupa: 'GG', luva: '11', bota: '43' },
  'col-bianca': { roupa: 'P', luva: '8', bota: '37' },
  'col-diego': { roupa: 'G', luva: '10', bota: '42' },
  'col-larissa': { roupa: 'M', luva: '8', bota: '38' },
  'col-eduardo': { roupa: 'G', luva: '9', bota: '41' },
  'col-gustavo': { roupa: 'M', luva: '9', bota: '42' },
  'col-renata': { roupa: 'P', luva: '7', bota: '36' },
  'col-thiago': { roupa: 'M', luva: '9', bota: '40' },
  'col-amanda': { roupa: 'P', luva: '7', bota: '38' },
  'col-bruno': { roupa: 'G', luva: '10', bota: '43' },
  'col-isabela': { roupa: 'M', luva: '8', bota: '37' },
};

export function tamanhoDoColaborador(colaboradorId: string, item: ItemEpiEpc): string | undefined {
  if (!item.possuiTamanho) return undefined;
  const t = TAMANHOS[colaboradorId] ?? { roupa: 'M', luva: '9', bota: '40' };
  if (item.categoriaId === 'cat-pes') return t.bota;
  if (item.categoriaId === 'cat-maos') return t.luva;
  return t.roupa;
}

const itemPorId = new Map(itens.map((i) => [i.id, i]));

// ---- Entregas (histórico já efetivado pelo almoxarifado) -----------------------

export function gerarEntregas(): Entrega[] {
  const entregas: Entrega[] = [];

  const entregar = (colaboradorId: string, motivoId: string, tipo: TipoMovimentacao, data: Date, ...lista: [string, number][]) => {
    entregas.push({
      id: `ent-${entregas.length + 1}`,
      colaboradorId,
      motivoId,
      tipoMovimentacao: tipo,
      status: 'Confirmada',
      dataHora: data.toISOString(),
      itens: lista.map(([itemId, quantidade]) => ({
        itemId,
        quantidade,
        tamanho: tamanhoDoColaborador(colaboradorId, itemPorId.get(itemId)!),
      })),
    });
  };

  // Roberto (usuário principal da demonstração): kit antigo, com um item vencido e um
  // vencendo, e uma troca recente de luva por defeito.
  entregar('col-roberto', 'mot-novo', 'EntregaInicial', diasAtras(700), ['itm-capacete', 1], ['itm-cinto', 1], ['itm-talabarte', 1]);
  entregar('col-roberto', 'mot-validade', 'Troca', diasAtras(385), ['itm-bota', 1]);
  entregar('col-roberto', 'mot-desgaste', 'Troca', diasAtras(340), ['itm-nomex', 2], ['itm-oculos', 1]);
  entregar('col-roberto', 'mot-desgaste', 'Troca', diasAtras(352), ['itm-luva-isolante', 2]);
  entregar('col-roberto', 'mot-defeito', 'Troca', diasAtras(16, 3), ['itm-luva-isolante', 1]);
  entregar('col-roberto', 'mot-novo', 'EntregaInicial', diasAtras(200), ['itm-cone', 4], ['itm-fita', 1]);

  // Demais colaboradores: kit inicial + reposições aleatórias, respeitando a elegibilidade.
  const aleatorio = criarAleatorio(42);
  for (const col of colaboradores) {
    if (col.id === 'col-roberto') continue;
    const permitidos = itensPermitidosPorCargo[col.cargoId];
    if (permitidos.length === 0) continue;

    const inicio = aleatorio.inteiro(200, 600);
    const kit = permitidos.filter(() => aleatorio.numero() < 0.7);
    entregar(col.id, 'mot-novo', 'EntregaInicial', diasAtras(inicio), ...kit.map((id) => [id, 1] as [string, number]));

    const trocas = aleatorio.inteiro(1, 4);
    for (let i = 0; i < trocas; i++) {
      const itemId = aleatorio.escolher(permitidos);
      entregar(col.id, aleatorio.escolher(['mot-desgaste', 'mot-avaria', 'mot-validade']), 'Troca', diasAtras(aleatorio.inteiro(5, inicio)), [itemId, 1]);
    }
  }

  return entregas.sort((a, b) => b.dataHora.localeCompare(a.dataHora));
}

// ---- Solicitações (pedidos feitos pelos colaboradores no app) --------------------

// Assinatura de exemplo (caminhos SVG em uma área de 300×120).
const ASSINATURA_DEMO =
  'M20 80 C 35 30, 55 30, 60 70 S 80 110, 95 60 M95 60 C 105 35, 120 40, 118 75 M130 72 C 140 40, 160 40, 165 70 C 170 95, 185 95, 195 60 M200 65 L 280 58';

function historicoAte(status: StatusSolicitacao, criadaEm: Date, unidadeSigla: string, motivoRecusa?: string): EventoSolicitacao[] {
  const passo = (horas: number) => new Date(criadaEm.getTime() + horas * 3_600_000).toISOString();
  const almox = `Almoxarifado ${unidadeSigla}`;
  const eventos: EventoSolicitacao[] = [{ status: 'Pendente', dataHora: criadaEm.toISOString(), responsavel: 'Você', comentario: 'Solicitação enviada pelo app.' }];
  if (status === 'Pendente') return eventos;
  eventos.push({ status: 'EmAnalise', dataHora: passo(3), responsavel: 'Carlos Eduardo Lima (SST)' });
  if (status === 'EmAnalise') return eventos;
  if (status === 'Recusada') {
    eventos.push({ status: 'Recusada', dataHora: passo(20), responsavel: 'Carlos Eduardo Lima (SST)', comentario: motivoRecusa });
    return eventos;
  }
  eventos.push({ status: 'Aprovada', dataHora: passo(20), responsavel: 'Carlos Eduardo Lima (SST)', comentario: `Retire o item no ${almox}.` });
  if (status === 'Aprovada') return eventos;
  eventos.push({ status: 'Entregue', dataHora: passo(46), responsavel: `${almox} — João Pedro Santos`, comentario: 'Item entregue e material antigo recolhido.' });
  return eventos;
}

export function gerarSolicitacoes(): Solicitacao[] {
  const lista: Solicitacao[] = [];
  const unidadeDe = (colaboradorId: string) => {
    const col = colaboradores.find((x) => x.id === colaboradorId)!;
    return unidades.find((u) => u.id === col.unidadeId)!;
  };

  const solicitar = (
    colaboradorId: string,
    itemId: string,
    motivoId: string,
    status: StatusSolicitacao,
    criadaEm: Date,
    relato: string,
    extra: { lote?: string; marca?: string; fabricacao?: string; quantidade?: number; recusa?: string; entregueHaDias?: number } = {},
  ) => {
    const item = itemPorId.get(itemId)!;
    const semDevolucao = motivos.find((m) => m.id === motivoId)?.semDevolucao;
    lista.push({
      id: `sol-${lista.length + 1}`,
      protocolo: '',
      colaboradorId,
      itemId,
      tamanho: tamanhoDoColaborador(colaboradorId, item),
      quantidade: extra.quantidade ?? 1,
      motivoId,
      materialAntigo: semDevolucao
        ? { relato }
        : { relato, lote: extra.lote, marca: extra.marca, fabricacao: extra.fabricacao, dataEntrega: diasAtras(extra.entregueHaDias ?? 300).toISOString() },
      fotos: [],
      assinatura: ASSINATURA_DEMO,
      status,
      criadaEm: criadaEm.toISOString(),
      historico: historicoAte(status, criadaEm, unidadeDe(colaboradorId).sigla, extra.recusa),
    });
  };

  // Roberto: uma solicitação em cada etapa, para a demonstração.
  solicitar('col-roberto', 'itm-bota', 'mot-validade', 'Pendente', diasAtras(0, 2), 'Bota passou da validade e o solado está descolando na ponta.', { lote: 'BT-2291', marca: 'Marluvas', fabricacao: '03/2024', entregueHaDias: 385 });
  solicitar('col-roberto', 'itm-oculos', 'mot-avaria', 'EmAnalise', diasAtras(1, 5), 'Lente riscou durante poda de árvore próxima à rede.', { lote: 'OC-7710', marca: 'Kalipso', fabricacao: '11/2024', entregueHaDias: 340 });
  solicitar('col-roberto', 'itm-nomex', 'mot-desgaste', 'Aprovada', diasAtras(4), 'Uniforme com costura aberta na manga e tecido desgastado.', { lote: 'NX-0415', marca: 'Protecta', fabricacao: '06/2024', quantidade: 2, entregueHaDias: 340 });
  solicitar('col-roberto', 'itm-luva-isolante', 'mot-defeito', 'Entregue', diasAtras(18), 'Luva apresentou fissura no teste de inflação antes do uso.', { lote: 'LI-3302', marca: 'Orion', fabricacao: '01/2025', entregueHaDias: 352 });
  solicitar('col-roberto', 'itm-capacete', 'mot-desgaste', 'Recusada', diasAtras(47), 'Capacete com a aba riscada.', {
    lote: 'CP-1187',
    marca: 'MSA',
    fabricacao: '02/2023',
    entregueHaDias: 700,
    recusa: 'Riscos superficiais não comprometem a proteção e o item está dentro da validade (vence em 2028).',
  });
  solicitar('col-roberto', 'itm-cone', 'mot-perda', 'Entregue', diasAtras(95), 'Um cone ficou na via após atendimento emergencial noturno e não foi localizado.');

  // Demais colaboradores: algumas solicitações aleatórias, sempre de itens elegíveis.
  const aleatorio = criarAleatorio(7);
  const relatos = [
    'Item desgastado pelo uso contínuo em campo.',
    'Material danificado durante atividade na rede.',
    'Item com validade vencida conforme etiqueta.',
    'Tamanho não ficou adequado após ajuste de uniforme.',
  ];
  for (const col of colaboradores) {
    if (col.id === 'col-roberto' || col.status === 'Inativo') continue;
    const permitidos = itensPermitidosPorCargo[col.cargoId];
    if (permitidos.length === 0) continue;
    const quantidade = aleatorio.inteiro(1, 4);
    for (let i = 0; i < quantidade; i++) {
      const dias = aleatorio.inteiro(0, 120);
      const status: StatusSolicitacao =
        dias < 3 ? aleatorio.escolher(['Pendente', 'EmAnalise'] as const) : dias < 10 ? aleatorio.escolher(['Aprovada', 'Entregue'] as const) : aleatorio.numero() < 0.15 ? 'Recusada' : 'Entregue';
      solicitar(col.id, aleatorio.escolher(permitidos), aleatorio.escolher(['mot-desgaste', 'mot-avaria', 'mot-validade', 'mot-tamanho']), status, diasAtras(dias, aleatorio.inteiro(0, 8)), aleatorio.escolher(relatos), {
        lote: `LT-${aleatorio.inteiro(1000, 9999)}`,
        fabricacao: `${String(aleatorio.inteiro(1, 13)).padStart(2, '0')}/${aleatorio.inteiro(2022, 2026)}`,
        recusa: 'Item dentro da validade e sem avaria que justifique a troca.',
      });
    }
  }

  // Protocolos em ordem cronológica, como seriam emitidos na vida real.
  lista.sort((a, b) => a.criadaEm.localeCompare(b.criadaEm)).forEach((s) => (s.protocolo = proximoProtocolo(new Date(s.criadaEm))));
  return lista.reverse();
}
