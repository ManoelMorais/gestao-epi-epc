import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import { useCallback, useMemo, useRef, useState } from 'react';
import { ActivityIndicator, FlatList, Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { CartaoSolicitacao } from '@/components/CartaoSolicitacao';
import { CampoTexto, ErroCarregamento, EstadoVazio } from '@/components/ui';
import { useSessaoAtiva } from '@/context/SessaoContext';
import { api, mensagemDeErro } from '@/services/api';
import { cores, espaco, visualStatusSolicitacao, type NomeIcone } from '@/theme/tema';
import type { GrupoStatus, SolicitacaoDetalhada } from '@/types/dominio';

const FILTROS: { grupo?: GrupoStatus; rotulo: string; icone?: NomeIcone }[] = [
  { rotulo: 'Todas' },
  { grupo: 'andamento', rotulo: 'Em andamento', icone: 'progress-clock' },
  { grupo: 'Aprovada', rotulo: 'Aprovadas', icone: visualStatusSolicitacao.Aprovada.icone },
  { grupo: 'Entregue', rotulo: 'Entregues', icone: visualStatusSolicitacao.Entregue.icone },
  { grupo: 'Recusada', rotulo: 'Recusadas', icone: visualStatusSolicitacao.Recusada.icone },
];

const GRUPOS_VALIDOS = FILTROS.map((f) => f.grupo).filter(Boolean) as string[];

export default function TelaSolicitacoes() {
  const sessao = useSessaoAtiva();
  const insets = useSafeAreaInsets();

  // O filtro de status vive na URL: os cartões do dashboard abrem esta aba já filtrada.
  const params = useLocalSearchParams<{ grupo?: string }>();
  const grupo = GRUPOS_VALIDOS.includes(params.grupo ?? '') ? (params.grupo as GrupoStatus) : undefined;
  const [termo, setTermo] = useState('');

  const [solicitacoes, setSolicitacoes] = useState<SolicitacaoDetalhada[] | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const requisicaoAtual = useRef(0);

  // Recarrega quando o filtro muda e sempre que a aba volta ao foco (ex.: depois de
  // enviar uma solicitação). A busca por texto espera um pouco para não consultar a cada tecla.
  const buscar = useCallback(async () => {
    // Descarta respostas antigas se o filtro mudar antes de a anterior voltar.
    const id = ++requisicaoAtual.current;
    try {
      const resultado = await api.listarSolicitacoes(sessao, { grupo, termo });
      if (id === requisicaoAtual.current) {
        setSolicitacoes(resultado);
        setErro(null);
      }
    } catch (e) {
      if (id === requisicaoAtual.current) setErro(mensagemDeErro(e));
    }
  }, [sessao, grupo, termo]);

  useFocusEffect(
    useCallback(() => {
      const t = setTimeout(buscar, termo ? 300 : 0);
      return () => clearTimeout(t);
    }, [buscar, termo]),
  );

  const contagem = useMemo(() => solicitacoes?.length ?? 0, [solicitacoes]);

  return (
    <View style={{ flex: 1, paddingTop: insets.top }}>
      <View style={estilos.cabecalho}>
        <View>
          <Text style={estilos.titulo}>Minhas solicitações</Text>
          <Text style={estilos.subtitulo}>{solicitacoes ? `${contagem} ${contagem === 1 ? 'solicitação' : 'solicitações'}` : erro ? 'Sem conexão' : 'Carregando…'}</Text>
        </View>

        <CampoTexto
          icone="magnify"
          placeholder="Buscar por item ou protocolo"
          value={termo}
          onChangeText={setTermo}
          returnKeyType="search"
          autoCorrect={false}
          direita={
            termo ? (
              <Pressable onPress={() => setTermo('')} hitSlop={10} accessibilityLabel="Limpar busca">
                <MaterialCommunityIcons name="close-circle" size={18} color={cores.inkMudo} />
              </Pressable>
            ) : undefined
          }
        />
      </View>

      <View>
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={estilos.filtros}>
          {FILTROS.map((f) => {
            const ativo = grupo === f.grupo;
            return (
              <Pressable
                key={f.rotulo}
                onPress={() => router.setParams({ grupo: f.grupo })}
                style={[estilos.chipFiltro, ativo && estilos.chipFiltroAtivo]}
                accessibilityRole="button"
                accessibilityState={{ selected: ativo }}
              >
                {f.icone && <MaterialCommunityIcons name={f.icone} size={15} color={ativo ? '#FFFFFF' : cores.inkSuave} />}
                <Text style={[estilos.chipFiltroTexto, ativo && { color: '#FFFFFF' }]}>{f.rotulo}</Text>
              </Pressable>
            );
          })}
        </ScrollView>
      </View>

      {erro ? (
        <ErroCarregamento mensagem={erro} onTentar={buscar} />
      ) : !solicitacoes ? (
        <ActivityIndicator color={cores.azul} style={{ marginTop: 60 }} />
      ) : (
        <FlatList
          data={solicitacoes}
          keyExtractor={(s) => s.id}
          renderItem={({ item }) => <CartaoSolicitacao solicitacao={item} />}
          ItemSeparatorComponent={() => <View style={{ height: espaco.m }} />}
          contentContainerStyle={{ paddingHorizontal: espaco.l, paddingTop: espaco.s, paddingBottom: 32 }}
          keyboardShouldPersistTaps="handled"
          keyboardDismissMode="on-drag"
          ListEmptyComponent={
            <EstadoVazio
              icone="clipboard-search-outline"
              titulo="Nenhuma solicitação encontrada"
              descricao={grupo || termo ? 'Tente outro termo de busca ou outro filtro.' : 'Suas solicitações de troca aparecerão aqui.'}
            />
          }
        />
      )}
    </View>
  );
}

const estilos = StyleSheet.create({
  cabecalho: { paddingHorizontal: espaco.l, paddingTop: espaco.l, gap: espaco.l },
  titulo: { fontSize: 28, fontWeight: '800', color: cores.ink, letterSpacing: -0.5 },
  subtitulo: { fontSize: 13.5, color: cores.inkSuave, marginTop: 2 },
  filtros: { paddingHorizontal: espaco.l, paddingVertical: espaco.m + 2, gap: espaco.s, alignItems: 'center' },
  chipFiltro: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    height: 36,
    paddingHorizontal: 14,
    borderRadius: 18,
    backgroundColor: cores.surface,
    borderWidth: 1,
    borderColor: cores.linha,
  },
  chipFiltroAtivo: { backgroundColor: cores.azul, borderColor: cores.azul },
  chipFiltroTexto: { fontSize: 13.5, fontWeight: '600', color: cores.inkSuave },
});
