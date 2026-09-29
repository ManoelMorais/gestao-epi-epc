import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { useRef, useState } from 'react';
import { Pressable, StyleSheet, Text, View, type GestureResponderEvent } from 'react-native';
import Svg, { Path } from 'react-native-svg';
import { cores, raio } from '@/theme/tema';

// A assinatura é guardada como caminhos SVG num espaço fixo de 300×120, independente do
// tamanho da tela em que foi desenhada — assim ela é exibida igual em qualquer aparelho
// (e no desktop). No backend real, esse texto viraria um PNG/SVG anexado à solicitação.
const LARGURA = 300;
const ALTURA = 120;

const arredondar = (n: number) => Math.round(n * 10) / 10;

export function PainelAssinatura({
  valor,
  onChange,
  onDesenhando,
}: {
  valor: string;
  onChange: (caminhos: string) => void;
  /** Avisa a tela para travar a rolagem enquanto o dedo está desenhando. */
  onDesenhando?: (desenhando: boolean) => void;
}) {
  const [tracos, setTracos] = useState<string[]>(valor ? [valor] : []);
  const [tracoAtual, setTracoAtual] = useState('');
  const tamanho = useRef({ largura: 1, altura: 1 });
  const atual = useRef('');

  const ponto = (e: GestureResponderEvent) => {
    const { locationX, locationY } = e.nativeEvent;
    const x = Math.min(LARGURA, Math.max(0, (locationX / tamanho.current.largura) * LARGURA));
    const y = Math.min(ALTURA, Math.max(0, (locationY / tamanho.current.altura) * ALTURA));
    return `${arredondar(x)} ${arredondar(y)}`;
  };

  function finalizar() {
    onDesenhando?.(false);
    // Um toque sem movimento vira um pontinho visível.
    const traco = atual.current.includes('L') ? atual.current : `${atual.current} l0.5 0.5`;
    atual.current = '';
    setTracoAtual('');
    const novos = [...tracos, traco];
    setTracos(novos);
    onChange(novos.join(' '));
  }

  // Props do sistema de "responder" do React Native: a View assume o toque e não o
  // devolve para a rolagem enquanto o dedo estiver desenhando.
  const responder = {
    onStartShouldSetResponder: () => true,
    onMoveShouldSetResponder: () => true,
    onResponderTerminationRequest: () => false,
    onResponderGrant: (e: GestureResponderEvent) => {
      onDesenhando?.(true);
      atual.current = `M${ponto(e)}`;
      setTracoAtual(atual.current);
    },
    onResponderMove: (e: GestureResponderEvent) => {
      atual.current += ` L${ponto(e)}`;
      setTracoAtual(atual.current);
    },
    onResponderRelease: finalizar,
    onResponderTerminate: finalizar,
  };

  function desfazer() {
    const novos = tracos.slice(0, -1);
    setTracos(novos);
    onChange(novos.join(' '));
  }

  function limpar() {
    setTracos([]);
    onChange('');
  }

  const vazio = tracos.length === 0 && !tracoAtual;

  return (
    <View style={{ gap: 8 }}>
      <View
        style={estilos.area}
        onLayout={(e) => (tamanho.current = { largura: e.nativeEvent.layout.width, altura: e.nativeEvent.layout.height })}
        {...responder}
      >
        {/* pointerEvents none: o toque sempre cai na View, com coordenadas relativas a ela. */}
        <Svg width="100%" height="100%" viewBox={`0 0 ${LARGURA} ${ALTURA}`} pointerEvents="none">
          {[...tracos, tracoAtual].filter(Boolean).map((d, i) => (
            <Path key={i} d={d} stroke={cores.azulEscuro} strokeWidth={2.4} fill="none" strokeLinecap="round" strokeLinejoin="round" />
          ))}
        </Svg>
        <View style={estilos.linhaBase} pointerEvents="none" />
        {vazio && (
          <View style={estilos.dica} pointerEvents="none">
            <MaterialCommunityIcons name="draw" size={20} color={cores.inkMudo} />
            <Text style={estilos.dicaTexto}>Assine aqui com o dedo</Text>
          </View>
        )}
      </View>
      <View style={estilos.acoes}>
        <Pressable onPress={desfazer} disabled={tracos.length === 0} style={estilos.acao} hitSlop={6}>
          <MaterialCommunityIcons name="undo" size={16} color={tracos.length ? cores.azul : cores.inkMudo} />
          <Text style={[estilos.acaoTexto, !tracos.length && { color: cores.inkMudo }]}>Desfazer</Text>
        </Pressable>
        <Pressable onPress={limpar} disabled={tracos.length === 0} style={estilos.acao} hitSlop={6}>
          <MaterialCommunityIcons name="eraser" size={16} color={tracos.length ? cores.vermelho : cores.inkMudo} />
          <Text style={[estilos.acaoTexto, { color: tracos.length ? cores.vermelho : cores.inkMudo }]}>Limpar</Text>
        </Pressable>
      </View>
    </View>
  );
}

/** Exibe uma assinatura salva (somente leitura). */
export function AssinaturaSalva({ caminhos }: { caminhos: string }) {
  return (
    <View style={[estilos.area, { backgroundColor: cores.surface }]}>
      <Svg width="100%" height="100%" viewBox={`0 0 ${LARGURA} ${ALTURA}`}>
        <Path d={caminhos} stroke={cores.azulEscuro} strokeWidth={2.4} fill="none" strokeLinecap="round" strokeLinejoin="round" />
      </Svg>
      <View style={estilos.linhaBase} />
    </View>
  );
}

const estilos = StyleSheet.create({
  area: {
    width: '100%',
    aspectRatio: LARGURA / ALTURA,
    borderRadius: raio.m,
    borderWidth: 1.5,
    borderStyle: 'dashed',
    borderColor: cores.linha,
    backgroundColor: '#FAFBFD',
    overflow: 'hidden',
  },
  linhaBase: { position: 'absolute', left: 20, right: 20, bottom: 26, height: 1, backgroundColor: cores.linha },
  dica: { position: 'absolute', top: 0, left: 0, right: 0, bottom: 0, alignItems: 'center', justifyContent: 'center', gap: 4 },
  dicaTexto: { fontSize: 13, color: cores.inkMudo, fontWeight: '600' },
  acoes: { flexDirection: 'row', justifyContent: 'flex-end', gap: 16 },
  acao: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  acaoTexto: { fontSize: 13, fontWeight: '700', color: cores.azul },
});
