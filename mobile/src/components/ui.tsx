import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { LinearGradient } from 'expo-linear-gradient';
import type { ReactNode, Ref } from 'react';
import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
  type StyleProp,
  type TextInputProps,
  type ViewStyle,
} from 'react-native';
import { cores, espaco, raio, sombra, type NomeIcone } from '@/theme/tema';
import { iniciais } from '@/utils/formatacao';

// Componentes visuais reutilizados pelas telas — equivalentes aos estilos nomeados
// do desktop (Cartao, Chip, Avatar, BotaoPrimario...).

export function Cartao({ children, style }: { children: ReactNode; style?: StyleProp<ViewStyle> }) {
  return <View style={[estilos.cartao, style]}>{children}</View>;
}

export function Chip({ rotulo, cor, fundo, icone }: { rotulo: string; cor: string; fundo: string; icone?: NomeIcone }) {
  return (
    <View style={[estilos.chip, { backgroundColor: fundo }]}>
      {icone && <MaterialCommunityIcons name={icone} size={12} color={cor} />}
      <Text style={[estilos.chipTexto, { color: cor }]} numberOfLines={1}>
        {rotulo}
      </Text>
    </View>
  );
}

const GRADIENTES_AVATAR: [string, string][] = [
  ['#3B82F6', '#1D4ED8'],
  ['#F97316', '#C2410C'],
  ['#10B981', '#047857'],
  ['#8B5CF6', '#6D28D9'],
  ['#06B6D4', '#0E7490'],
  ['#F43F5E', '#BE123C'],
];

export function Avatar({ nome, tamanho = 40 }: { nome: string; tamanho?: number }) {
  // A cor é derivada do nome: a mesma pessoa tem sempre o mesmo avatar.
  const indice = [...nome].reduce((soma, c) => soma + c.charCodeAt(0), 0) % GRADIENTES_AVATAR.length;
  return (
    <LinearGradient
      colors={GRADIENTES_AVATAR[indice]}
      start={{ x: 0, y: 0 }}
      end={{ x: 1, y: 1 }}
      style={{ width: tamanho, height: tamanho, borderRadius: tamanho / 2, alignItems: 'center', justifyContent: 'center' }}
    >
      <Text style={{ color: '#FFFFFF', fontWeight: '700', fontSize: tamanho * 0.36 }}>{iniciais(nome)}</Text>
    </LinearGradient>
  );
}

export function IconeRedondo({ icone, cor, fundo, tamanho = 40 }: { icone: NomeIcone; cor: string; fundo: string; tamanho?: number }) {
  return (
    <View style={{ width: tamanho, height: tamanho, borderRadius: tamanho * 0.32, backgroundColor: fundo, alignItems: 'center', justifyContent: 'center' }}>
      <MaterialCommunityIcons name={icone} size={tamanho * 0.52} color={cor} />
    </View>
  );
}

type VarianteBotao = 'primario' | 'secundario' | 'texto';

export function Botao({
  titulo,
  onPress,
  icone,
  variante = 'primario',
  carregando,
  desabilitado,
  style,
}: {
  titulo: string;
  onPress: () => void;
  icone?: NomeIcone;
  variante?: VarianteBotao;
  carregando?: boolean;
  desabilitado?: boolean;
  style?: StyleProp<ViewStyle>;
}) {
  const inativo = desabilitado || carregando;
  const corTexto = variante === 'primario' ? '#FFFFFF' : cores.azul;
  return (
    <Pressable
      onPress={onPress}
      disabled={inativo}
      style={({ pressed }) => [
        estilos.botao,
        variante === 'primario' && estilos.botaoPrimario,
        variante === 'secundario' && estilos.botaoSecundario,
        variante === 'texto' && estilos.botaoTexto,
        inativo && { opacity: 0.5 },
        pressed && { opacity: 0.85, transform: [{ scale: 0.99 }] },
        style,
      ]}
    >
      {carregando ? (
        <ActivityIndicator color={corTexto} />
      ) : (
        <>
          {icone && <MaterialCommunityIcons name={icone} size={20} color={corTexto} />}
          <Text style={[estilos.botaoRotulo, { color: corTexto }]}>{titulo}</Text>
        </>
      )}
    </Pressable>
  );
}

export function CampoTexto({
  rotulo,
  icone,
  erro,
  direita,
  ref,
  ...props
}: TextInputProps & { rotulo?: string; icone?: NomeIcone; erro?: boolean; direita?: ReactNode; ref?: Ref<TextInput> }) {
  return (
    <View style={{ gap: 6 }}>
      {rotulo && <Text style={estilos.rotuloCampo}>{rotulo}</Text>}
      <View style={[estilos.campo, erro && { borderColor: cores.vermelho }]}>
        {icone && <MaterialCommunityIcons name={icone} size={20} color={cores.inkMudo} />}
        <TextInput ref={ref} placeholderTextColor={cores.inkMudo} style={estilos.campoInput} {...props} />
        {direita}
      </View>
    </View>
  );
}

export function TituloSecao({ titulo, acao }: { titulo: string; acao?: ReactNode }) {
  return (
    <View style={estilos.tituloSecao}>
      <Text style={estilos.tituloSecaoTexto}>{titulo}</Text>
      {acao}
    </View>
  );
}

export function EstadoVazio({ icone, titulo, descricao }: { icone: NomeIcone; titulo: string; descricao?: string }) {
  return (
    <View style={estilos.vazio}>
      <IconeRedondo icone={icone} cor={cores.inkMudo} fundo={cores.linha} tamanho={56} />
      <Text style={estilos.vazioTitulo}>{titulo}</Text>
      {descricao && <Text style={estilos.vazioDescricao}>{descricao}</Text>}
    </View>
  );
}

export function Aviso({ tipo, texto }: { tipo: 'erro' | 'alerta' | 'info'; texto: string }) {
  const visual = {
    erro: { cor: cores.vermelho, fundo: cores.vermelhoSuave, icone: 'alert-circle' as const },
    alerta: { cor: cores.laranja, fundo: cores.laranjaSuave, icone: 'alert' as const },
    info: { cor: cores.azul, fundo: cores.azulSuave, icone: 'information' as const },
  }[tipo];
  return (
    <View style={[estilos.aviso, { backgroundColor: visual.fundo }]}>
      <MaterialCommunityIcons name={visual.icone} size={18} color={visual.cor} />
      <Text style={[estilos.avisoTexto, { color: visual.cor }]}>{texto}</Text>
    </View>
  );
}

const estilos = StyleSheet.create({
  cartao: {
    backgroundColor: cores.surface,
    borderRadius: raio.g,
    padding: espaco.l,
    ...sombra,
  },
  chip: {
    flexDirection: 'row',
    alignItems: 'center',
    alignSelf: 'flex-start',
    gap: 4,
    paddingHorizontal: 8,
    paddingVertical: 3,
    borderRadius: 6,
  },
  chipTexto: { fontSize: 11.5, fontWeight: '700' },
  botao: {
    height: 52,
    borderRadius: raio.m,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: espaco.s,
    paddingHorizontal: espaco.xl,
  },
  botaoPrimario: { backgroundColor: cores.azul, ...sombra, shadowColor: cores.azul, shadowOpacity: 0.3 },
  botaoSecundario: { backgroundColor: cores.azulSuave },
  botaoTexto: { height: 40, paddingHorizontal: espaco.s },
  botaoRotulo: { fontSize: 15.5, fontWeight: '700' },
  rotuloCampo: { fontSize: 13, fontWeight: '600', color: cores.inkSuave },
  campo: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    height: 52,
    paddingHorizontal: 14,
    borderRadius: raio.m,
    borderWidth: 1.5,
    borderColor: cores.linha,
    backgroundColor: cores.surface,
  },
  campoInput: { flex: 1, fontSize: 15.5, color: cores.ink, height: '100%' },
  tituloSecao: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', marginBottom: espaco.m },
  tituloSecaoTexto: { fontSize: 17, fontWeight: '700', color: cores.ink },
  vazio: { alignItems: 'center', paddingVertical: 48, paddingHorizontal: 32, gap: 10 },
  vazioTitulo: { fontSize: 16, fontWeight: '700', color: cores.ink, textAlign: 'center' },
  vazioDescricao: { fontSize: 14, color: cores.inkSuave, textAlign: 'center', lineHeight: 20 },
  aviso: { flexDirection: 'row', alignItems: 'flex-start', gap: 10, padding: 12, borderRadius: raio.m },
  avisoTexto: { flex: 1, fontSize: 13.5, fontWeight: '600', lineHeight: 19 },
});

/** Estado de falha ao carregar dados (sem internet, servidor fora do ar...), com nova tentativa. */
export function ErroCarregamento({ mensagem, onTentar }: { mensagem: string; onTentar: () => void }) {
  return (
    <View style={estilos.vazio}>
      <IconeRedondo icone="wifi-off" cor={cores.vermelho} fundo={cores.vermelhoSuave} tamanho={56} />
      <Text style={estilos.vazioTitulo}>Não foi possível carregar</Text>
      <Text style={estilos.vazioDescricao}>{mensagem}</Text>
      <Botao titulo="Tentar de novo" icone="refresh" variante="secundario" onPress={onTentar} style={{ marginTop: 8, alignSelf: 'stretch' }} />
    </View>
  );
}
