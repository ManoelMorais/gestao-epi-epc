import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { router } from 'expo-router';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { cores, espaco, iconeCategoria, raio, sombra, visualStatusSolicitacao } from '@/theme/tema';
import type { SolicitacaoDetalhada } from '@/types/dominio';
import { formatarHora, rotuloRelativo } from '@/utils/formatacao';
import { Chip, IconeRedondo } from './ui';

export function CartaoSolicitacao({ solicitacao }: { solicitacao: SolicitacaoDetalhada }) {
  const status = visualStatusSolicitacao[solicitacao.status];
  const detalhes = [`Qtd. ${solicitacao.quantidade}`, solicitacao.tamanho && `Tam. ${solicitacao.tamanho}`, solicitacao.motivo.descricao].filter(Boolean).join(' · ');

  return (
    <Pressable
      onPress={() => router.push({ pathname: '/solicitacao/[id]', params: { id: solicitacao.id } })}
      style={({ pressed }) => [estilos.cartao, pressed && { opacity: 0.9, transform: [{ scale: 0.995 }] }]}
    >
      <View style={[estilos.faixa, { backgroundColor: status.cor }]} />
      <View style={estilos.corpo}>
        <View style={estilos.linhaTopo}>
          <IconeRedondo icone={iconeCategoria(solicitacao.item.categoriaId)} cor={cores.azul} fundo={cores.azulSuave} tamanho={40} />
          <View style={{ flex: 1 }}>
            <Text style={estilos.nome} numberOfLines={2}>
              {solicitacao.item.nome}
            </Text>
            <Text style={estilos.meta} numberOfLines={1}>
              {detalhes}
            </Text>
          </View>
        </View>

        <View style={estilos.linhaRodape}>
          <Chip rotulo={status.rotulo} cor={status.cor} fundo={status.fundo} icone={status.icone} />
          <View style={{ flex: 1 }} />
          <Text style={estilos.protocolo}>
            {solicitacao.protocolo} · {rotuloRelativo(solicitacao.criadaEm)}, {formatarHora(solicitacao.criadaEm)}
          </Text>
          <MaterialCommunityIcons name="chevron-right" size={18} color={cores.inkMudo} />
        </View>
      </View>
    </Pressable>
  );
}

const estilos = StyleSheet.create({
  cartao: {
    flexDirection: 'row',
    backgroundColor: cores.surface,
    borderRadius: raio.m + 2,
    overflow: 'hidden',
    ...sombra,
  },
  faixa: { width: 4 },
  corpo: { flex: 1, padding: espaco.m + 2, gap: 12 },
  linhaTopo: { flexDirection: 'row', alignItems: 'center', gap: 12 },
  nome: { fontSize: 15, fontWeight: '700', color: cores.ink },
  meta: { fontSize: 12.5, color: cores.inkSuave, marginTop: 2 },
  linhaRodape: { flexDirection: 'row', alignItems: 'center', gap: 6 },
  protocolo: { fontSize: 11.5, color: cores.inkMudo, fontWeight: '600' },
});
