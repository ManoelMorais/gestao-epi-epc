import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { router } from 'expo-router';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { cores, iconeCategoria, visualValidade } from '@/theme/tema';
import type { ItemEmPosse } from '@/types/dominio';
import { formatarData } from '@/utils/formatacao';
import { Chip, IconeRedondo } from './ui';

/** Um EPI/EPC que está com o colaborador. Tocar abre a solicitação de troca já com o item escolhido. */
export function LinhaItemEmPosse({ posse }: { posse: ItemEmPosse }) {
  const validade = visualValidade(posse);
  const detalhes = [`${posse.quantidade} un.`, posse.tamanho && `Tam. ${posse.tamanho}`, `recebido em ${formatarData(posse.recebidoEm)}`].filter(Boolean).join(' · ');

  return (
    <Pressable
      onPress={() => router.navigate({ pathname: '/nova', params: { item: posse.item.id } })}
      style={({ pressed }) => [estilos.linha, pressed && { backgroundColor: cores.paper }]}
      accessibilityHint="Abre uma solicitação de troca deste item"
    >
      <IconeRedondo icone={iconeCategoria(posse.item.categoriaId)} cor={validade.cor} fundo={validade.fundo} tamanho={40} />
      <View style={{ flex: 1, gap: 4 }}>
        <Text style={estilos.nome} numberOfLines={1}>
          {posse.item.nome}
        </Text>
        <Text style={estilos.meta} numberOfLines={1}>
          {detalhes}
        </Text>
        <Chip rotulo={validade.rotulo} cor={validade.cor} fundo={validade.fundo} icone={validade.icone} />
      </View>
      <View style={estilos.trocar}>
        <MaterialCommunityIcons name="swap-horizontal" size={18} color={cores.azul} />
      </View>
    </Pressable>
  );
}

const estilos = StyleSheet.create({
  linha: { flexDirection: 'row', alignItems: 'center', gap: 12, paddingVertical: 10, paddingHorizontal: 4, borderRadius: 12 },
  nome: { fontSize: 14.5, fontWeight: '700', color: cores.ink },
  meta: { fontSize: 12, color: cores.inkSuave },
  trocar: { width: 34, height: 34, borderRadius: 10, backgroundColor: cores.azulSuave, alignItems: 'center', justifyContent: 'center' },
});
