import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { LinearGradient } from 'expo-linear-gradient';
import { router, useFocusEffect } from 'expo-router';
import { useCallback, useState } from 'react';
import { ActivityIndicator, Pressable, RefreshControl, ScrollView, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { CartaoSolicitacao } from '@/components/CartaoSolicitacao';
import { LinhaItemEmPosse } from '@/components/LinhaItemEmPosse';
import { Avatar, Aviso, Cartao, ErroCarregamento, EstadoVazio, IconeRedondo, TituloSecao } from '@/components/ui';
import { useSessaoAtiva } from '@/context/SessaoContext';
import { api, mensagemDeErro } from '@/services/api';
import { cores, DIAS_ALERTA_VALIDADE, espaco, gradienteMarca, raio, sombra, visualStatusSolicitacao, type NomeIcone } from '@/theme/tema';
import type { GrupoStatus, ResumoColaborador } from '@/types/dominio';
import { primeiroNome, saudacao } from '@/utils/formatacao';

const ITENS_VISIVEIS = 4;

export default function TelaDashboard() {
  const sessao = useSessaoAtiva();
  const { colaborador } = sessao;
  const insets = useSafeAreaInsets();

  const [resumo, setResumo] = useState<ResumoColaborador | null>(null);
  const [atualizando, setAtualizando] = useState(false);
  const [mostrarTodos, setMostrarTodos] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  const carregar = useCallback(async () => {
    try {
      setResumo(await api.obterResumo(sessao));
      setErro(null);
    } catch (e) {
      setErro(mensagemDeErro(e));
    }
  }, [sessao]);

  // Recarrega sempre que a aba volta ao foco — assim uma solicitação nova aparece na hora.
  useFocusEffect(
    useCallback(() => {
      carregar();
    }, [carregar]),
  );

  async function aoPuxar() {
    setAtualizando(true);
    await carregar();
    setAtualizando(false);
  }

  const aprovada = resumo?.recentes.find((s) => s.status === 'Aprovada');
  const itens = resumo?.itensEmPosse ?? [];
  const vencidos = itens.filter((i) => i.diasParaVencer !== undefined && i.diasParaVencer <= 0).length;
  const vencendo = itens.filter((i) => i.diasParaVencer !== undefined && i.diasParaVencer > 0 && i.diasParaVencer <= DIAS_ALERTA_VALIDADE).length;
  const emDia = itens.length - vencidos - vencendo;

  return (
    <ScrollView
      style={{ flex: 1 }}
      contentContainerStyle={{ paddingBottom: 32 }}
      refreshControl={<RefreshControl refreshing={atualizando} onRefresh={aoPuxar} tintColor="#FFFFFF" progressViewOffset={insets.top} />}
    >
      <LinearGradient colors={gradienteMarca} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }} style={[estilos.topo, { paddingTop: insets.top + 16 }]}>
        <View style={[estilos.circulo, { width: 220, height: 220, top: -70, right: -60 }]} />

        <View style={estilos.linhaTopo}>
          <Avatar nome={colaborador.nome} tamanho={48} />
          <View style={{ flex: 1 }}>
            <Text style={estilos.saudacao}>{saudacao()},</Text>
            <Text style={estilos.nome}>{primeiroNome(colaborador.nome)}</Text>
          </View>
        </View>

        <Text style={estilos.cargo}>{colaborador.cargo.nome}</Text>
        <View style={estilos.linhaEtiquetas}>
          <Etiqueta icone="card-account-details-outline" texto={`DRT ${colaborador.drt}`} />
          <Etiqueta icone="map-marker-outline" texto={`${colaborador.unidade.nome} · ${colaborador.unidade.sigla}`} />
        </View>
      </LinearGradient>

      {!resumo && erro ? (
        <View style={estilos.conteudo}>
          <Cartao>
            <ErroCarregamento mensagem={erro} onTentar={carregar} />
          </Cartao>
        </View>
      ) : !resumo ? (
        <ActivityIndicator color={cores.azul} style={{ marginTop: 80 }} />
      ) : (
        <View style={estilos.conteudo}>
          {/* Resumo dos EPIs que estão com o colaborador */}
          <Cartao style={{ padding: espaco.xl }}>
            <View style={{ flexDirection: 'row', alignItems: 'flex-start' }}>
              <View style={{ flex: 1 }}>
                <Text style={estilos.destaqueRotulo}>EPI/EPC com você</Text>
                <Text style={estilos.destaqueValor}>{itens.length}</Text>
                <Text style={estilos.destaqueSub}>{itens.length === 1 ? 'item' : 'itens'} sob sua responsabilidade</Text>
              </View>
              <IconeRedondo icone="shield-account" cor={cores.azul} fundo={cores.azulSuave} tamanho={44} />
            </View>

            {itens.length > 0 && (
              <>
                {/* Barra de proporção: vencidos / vencendo / em dia (com rótulos abaixo, nunca só cor). */}
                <View style={estilos.barraValidade}>
                  {vencidos > 0 && <View style={[estilos.segmento, { flex: vencidos, backgroundColor: cores.vermelho }]} />}
                  {vencendo > 0 && <View style={[estilos.segmento, { flex: vencendo, backgroundColor: cores.laranja }]} />}
                  {emDia > 0 && <View style={[estilos.segmento, { flex: emDia, backgroundColor: cores.verde }]} />}
                </View>
                <View style={estilos.linhaLegenda}>
                  <Legenda cor={cores.vermelho} icone="alert-octagon" valor={vencidos} rotulo="Vencidos" />
                  <Legenda cor={cores.laranja} icone="clock-alert-outline" valor={vencendo} rotulo={`Vencem em ${DIAS_ALERTA_VALIDADE}d`} />
                  <Legenda cor={cores.verde} icone="shield-check" valor={emDia} rotulo="Em dia" />
                </View>
              </>
            )}
          </Cartao>

          {colaborador.status === 'Afastado' && (
            <Aviso tipo="alerta" texto="Você está afastado. Suas solicitações serão analisadas quando você retornar às atividades." />
          )}

          {aprovada && (
            <Pressable onPress={() => router.push({ pathname: '/solicitacao/[id]', params: { id: aprovada.id } })}>
              <View style={estilos.bannerAprovada}>
                <IconeRedondo icone="check-decagram" cor="#FFFFFF" fundo={cores.azul} tamanho={40} />
                <View style={{ flex: 1 }}>
                  <Text style={estilos.bannerTitulo}>Troca aprovada — pode retirar!</Text>
                  <Text style={estilos.bannerTexto} numberOfLines={2}>
                    {aprovada.item.nome} no Almoxarifado {colaborador.unidade.sigla}. Leve o item antigo.
                  </Text>
                </View>
                <MaterialCommunityIcons name="chevron-right" size={22} color={cores.azul} />
              </View>
            </Pressable>
          )}

          <Pressable onPress={() => router.navigate('/nova')} style={({ pressed }) => [pressed && { opacity: 0.9 }]}>
            <LinearGradient colors={[cores.laranja, '#C2410C']} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }} style={estilos.cta}>
              <View style={estilos.ctaIcone}>
                <MaterialCommunityIcons name="swap-horizontal" size={26} color={cores.laranja} />
              </View>
              <View style={{ flex: 1 }}>
                <Text style={estilos.ctaTitulo}>Solicitar troca de EPI/EPC</Text>
                <Text style={estilos.ctaTexto}>Desgaste, dano, validade ou perda</Text>
              </View>
              <MaterialCommunityIcons name="arrow-right" size={22} color="#FFFFFF" />
            </LinearGradient>
          </Pressable>

          {/* Situação das minhas solicitações */}
          <View>
            <TituloSecao titulo="Minhas solicitações" />
            <View style={estilos.gradeStatus}>
              <TileStatus grupo="andamento" valor={resumo.porStatus.Pendente + resumo.porStatus.EmAnalise} rotulo="Em andamento" icone="progress-clock" cor={cores.roxo} fundo={cores.roxoSuave} />
              <TileStatus grupo="Aprovada" valor={resumo.porStatus.Aprovada} rotulo="Aprovadas" {...visualDoStatus('Aprovada')} />
              <TileStatus grupo="Entregue" valor={resumo.porStatus.Entregue} rotulo="Entregues" {...visualDoStatus('Entregue')} />
              <TileStatus grupo="Recusada" valor={resumo.porStatus.Recusada} rotulo="Recusadas" {...visualDoStatus('Recusada')} />
            </View>
          </View>

          {/* Meus EPIs */}
          <Cartao>
            <TituloSecao titulo="Meus EPIs" acao={<Text style={estilos.dicaSecao}>Toque para trocar</Text>} />
            {itens.length === 0 ? (
              <EstadoVazio
                icone="shield-off-outline"
                titulo="Nenhum EPI/EPC registrado com você"
                descricao="Quando o almoxarifado registrar uma entrega para você, ela aparece aqui."
              />
            ) : (
              <View>
                {(mostrarTodos ? itens : itens.slice(0, ITENS_VISIVEIS)).map((posse, i) => (
                  <View key={posse.item.id}>
                    {i > 0 && <View style={estilos.divisor} />}
                    <LinhaItemEmPosse posse={posse} />
                  </View>
                ))}
                {itens.length > ITENS_VISIVEIS && (
                  <Pressable onPress={() => setMostrarTodos((v) => !v)} style={estilos.verMais} hitSlop={6}>
                    <Text style={estilos.link}>{mostrarTodos ? 'Mostrar menos' : `Ver todos (${itens.length})`}</Text>
                    <MaterialCommunityIcons name={mostrarTodos ? 'chevron-up' : 'chevron-down'} size={18} color={cores.azul} />
                  </Pressable>
                )}
              </View>
            )}
          </Cartao>

          <View>
            <TituloSecao
              titulo="Últimas solicitações"
              acao={
                <Pressable onPress={() => router.navigate('/solicitacoes')} hitSlop={8}>
                  <Text style={estilos.link}>Ver todas</Text>
                </Pressable>
              }
            />
            {resumo.recentes.length === 0 ? (
              <EstadoVazio icone="clipboard-text-outline" titulo="Você ainda não fez solicitações" descricao="Quando precisar trocar um EPI/EPC, é só tocar em “Solicitar troca”." />
            ) : (
              <View style={{ gap: espaco.m }}>
                {resumo.recentes.map((s) => (
                  <CartaoSolicitacao key={s.id} solicitacao={s} />
                ))}
              </View>
            )}
          </View>
        </View>
      )}
    </ScrollView>
  );
}

function visualDoStatus(status: 'Aprovada' | 'Entregue' | 'Recusada') {
  const v = visualStatusSolicitacao[status];
  return { icone: v.icone, cor: v.cor, fundo: v.fundo };
}

function Etiqueta({ icone, texto }: { icone: NomeIcone; texto: string }) {
  return (
    <View style={estilos.etiqueta}>
      <MaterialCommunityIcons name={icone} size={14} color="#FFFFFF" />
      <Text style={estilos.etiquetaTexto}>{texto}</Text>
    </View>
  );
}

function Legenda({ cor, icone, valor, rotulo }: { cor: string; icone: NomeIcone; valor: number; rotulo: string }) {
  return (
    <View style={{ flex: 1, gap: 2 }}>
      <View style={{ flexDirection: 'row', alignItems: 'center', gap: 4 }}>
        <MaterialCommunityIcons name={icone} size={14} color={cor} />
        <Text style={estilos.legendaValor}>{valor}</Text>
      </View>
      <Text style={estilos.legendaRotulo}>{rotulo}</Text>
    </View>
  );
}

function TileStatus({ grupo, valor, rotulo, icone, cor, fundo }: { grupo: GrupoStatus; valor: number; rotulo: string; icone: NomeIcone; cor: string; fundo: string }) {
  return (
    <Pressable
      onPress={() => router.navigate({ pathname: '/solicitacoes', params: { grupo } })}
      style={({ pressed }) => [estilos.tile, pressed && { opacity: 0.85 }]}
      accessibilityLabel={`${rotulo}: ${valor}`}
    >
      <IconeRedondo icone={icone} cor={cor} fundo={fundo} tamanho={32} />
      <Text style={estilos.tileValor}>{valor}</Text>
      <Text style={estilos.tileRotulo} numberOfLines={1}>
        {rotulo}
      </Text>
    </Pressable>
  );
}

const estilos = StyleSheet.create({
  topo: { paddingHorizontal: espaco.xl, paddingBottom: 64, overflow: 'hidden' },
  circulo: { position: 'absolute', borderRadius: 999, backgroundColor: 'rgba(255,255,255,0.08)' },
  linhaTopo: { flexDirection: 'row', alignItems: 'center', gap: 12 },
  saudacao: { color: 'rgba(255,255,255,0.8)', fontSize: 14 },
  nome: { color: '#FFFFFF', fontSize: 22, fontWeight: '800' },
  cargo: { color: '#FFFFFF', fontSize: 14.5, fontWeight: '600', marginTop: 14 },
  linhaEtiquetas: { flexDirection: 'row', flexWrap: 'wrap', gap: 8, marginTop: 8 },
  etiqueta: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingHorizontal: 10,
    paddingVertical: 5,
    borderRadius: 8,
    backgroundColor: 'rgba(255,255,255,0.16)',
  },
  etiquetaTexto: { color: '#FFFFFF', fontSize: 12.5, fontWeight: '600' },
  conteudo: { marginTop: -44, paddingHorizontal: espaco.l, gap: espaco.l },
  destaqueRotulo: { fontSize: 13, fontWeight: '600', color: cores.inkSuave },
  destaqueValor: { fontSize: 40, fontWeight: '800', color: cores.ink, marginTop: 2, letterSpacing: -1 },
  destaqueSub: { fontSize: 13, color: cores.inkSuave },
  barraValidade: { flexDirection: 'row', height: 10, gap: 2, marginTop: 18, borderRadius: 5, overflow: 'hidden' },
  segmento: { height: '100%' },
  linhaLegenda: { flexDirection: 'row', marginTop: 12 },
  legendaValor: { fontSize: 16, fontWeight: '800', color: cores.ink },
  legendaRotulo: { fontSize: 11.5, color: cores.inkSuave, fontWeight: '600' },
  bannerAprovada: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
    padding: 14,
    borderRadius: raio.g,
    backgroundColor: cores.azulSuave,
    borderWidth: 1,
    borderColor: '#C7D6FB',
  },
  bannerTitulo: { fontSize: 15, fontWeight: '800', color: cores.azulEscuro },
  bannerTexto: { fontSize: 13, color: cores.inkSuave, marginTop: 2, lineHeight: 18 },
  cta: { flexDirection: 'row', alignItems: 'center', gap: 14, padding: 16, borderRadius: raio.g, ...sombra, shadowColor: cores.laranja, shadowOpacity: 0.3 },
  ctaIcone: { width: 44, height: 44, borderRadius: 14, backgroundColor: '#FFFFFF', alignItems: 'center', justifyContent: 'center' },
  ctaTitulo: { color: '#FFFFFF', fontSize: 16, fontWeight: '800' },
  ctaTexto: { color: 'rgba(255,255,255,0.88)', fontSize: 13, marginTop: 2 },
  gradeStatus: { flexDirection: 'row', flexWrap: 'wrap', gap: espaco.m },
  tile: { flexGrow: 1, flexBasis: '46%', padding: 14, gap: 4, backgroundColor: cores.surface, borderRadius: raio.g, ...sombra },
  tileValor: { fontSize: 24, fontWeight: '800', color: cores.ink, marginTop: 6 },
  tileRotulo: { fontSize: 12.5, color: cores.inkSuave, fontWeight: '600' },
  dicaSecao: { fontSize: 12, color: cores.inkMudo, fontWeight: '600' },
  divisor: { height: 1, backgroundColor: cores.linha, marginLeft: 56 },
  verMais: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: 4, paddingTop: 12 },
  link: { fontSize: 14, fontWeight: '700', color: cores.azul },
});
