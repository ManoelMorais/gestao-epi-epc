/** Minúsculas e sem acentos, para buscas tolerantes ("patricia" encontra "Patrícia"). */
export function normalizarTexto(texto: string) {
  return texto
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .trim();
}

export function inicioDoDia(data: Date) {
  const d = new Date(data);
  d.setHours(0, 0, 0, 0);
  return d;
}

export function iniciais(nome: string) {
  const partes = nome.trim().split(/\s+/);
  const primeira = partes[0]?.[0] ?? '';
  const ultima = partes.length > 1 ? partes[partes.length - 1][0] : '';
  return (primeira + ultima).toUpperCase();
}

export function primeiroNome(nome: string) {
  return nome.trim().split(/\s+/)[0];
}

const DIAS_SEMANA = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];
const MESES = ['jan', 'fev', 'mar', 'abr', 'mai', 'jun', 'jul', 'ago', 'set', 'out', 'nov', 'dez'];

const doisDigitos = (n: number) => String(n).padStart(2, '0');

export function formatarHora(iso: string) {
  const d = new Date(iso);
  return `${doisDigitos(d.getHours())}:${doisDigitos(d.getMinutes())}`;
}

export function formatarDataCurta(iso: string) {
  const d = new Date(iso);
  return `${doisDigitos(d.getDate())} ${MESES[d.getMonth()]}`;
}

export function formatarDataCompleta(iso: string) {
  const d = new Date(iso);
  return `${doisDigitos(d.getDate())}/${doisDigitos(d.getMonth() + 1)}/${d.getFullYear()} às ${formatarHora(iso)}`;
}

export function diaDaSemanaCurto(iso: string) {
  return DIAS_SEMANA[new Date(iso).getDay()];
}

/** "Hoje", "Ontem" ou a data curta, para agrupar e rotular listas. */
export function rotuloRelativo(iso: string) {
  const hoje = inicioDoDia(new Date()).getTime();
  const dia = inicioDoDia(new Date(iso)).getTime();
  const diferencaDias = Math.round((hoje - dia) / 86_400_000);
  if (diferencaDias === 0) return 'Hoje';
  if (diferencaDias === 1) return 'Ontem';
  if (diferencaDias < 7) return `${DIAS_SEMANA[new Date(iso).getDay()]}, ${formatarDataCurta(iso)}`;
  return formatarDataCurta(iso) + (new Date(iso).getFullYear() !== new Date().getFullYear() ? ` ${new Date(iso).getFullYear()}` : '');
}

export function saudacao(data = new Date()) {
  const h = data.getHours();
  if (h < 12) return 'Bom dia';
  if (h < 18) return 'Boa tarde';
  return 'Boa noite';
}

export function formatarData(iso: string) {
  const d = new Date(iso);
  return `${doisDigitos(d.getDate())}/${doisDigitos(d.getMonth() + 1)}/${d.getFullYear()}`;
}

/** Máscara "dd/mm/aaaa" enquanto o usuário digita. */
export function mascaraData(texto: string) {
  const d = texto.replace(/\D/g, '').slice(0, 8);
  if (d.length <= 2) return d;
  if (d.length <= 4) return `${d.slice(0, 2)}/${d.slice(2)}`;
  return `${d.slice(0, 2)}/${d.slice(2, 4)}/${d.slice(4)}`;
}

/** Máscara "mm/aaaa" enquanto o usuário digita. */
export function mascaraMesAno(texto: string) {
  const d = texto.replace(/\D/g, '').slice(0, 6);
  if (d.length <= 2) return d;
  return `${d.slice(0, 2)}/${d.slice(2)}`;
}

/** Converte "dd/mm/aaaa" em data; `null` se for inválida (ex.: 31/02) ou estiver no futuro. */
export function lerData(texto: string): Date | null {
  const m = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(texto);
  if (!m) return null;
  const [dia, mes, ano] = [Number(m[1]), Number(m[2]), Number(m[3])];
  const data = new Date(ano, mes - 1, dia);
  if (data.getDate() !== dia || data.getMonth() !== mes - 1 || ano < 2000) return null;
  return data > new Date() ? null : data;
}

/** Valida "mm/aaaa" de fabricação: mês real, não futuro e não absurdamente antigo. */
export function mesAnoValido(texto: string) {
  const m = /^(\d{2})\/(\d{4})$/.exec(texto);
  if (!m) return false;
  const [mes, ano] = [Number(m[1]), Number(m[2])];
  if (mes < 1 || mes > 12 || ano < 2000) return false;
  return new Date(ano, mes - 1, 1) <= new Date();
}
