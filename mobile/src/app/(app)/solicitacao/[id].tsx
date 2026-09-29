import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { useLocalSearchParams } from 'expo-router';
import { useEffect, useState } from 'react';
import { ActivityIndicator, Image, ScrollView, StyleSheet, Text, View } from 'react-native';
import { AssinaturaSalva } from '@/components/Assinatura';
import { Aviso, Cartao, Chip, EstadoVazio, IconeRedondo, TituloSecao } from '@/components/ui';
import { useSessaoAtiva } from '@/context/SessaoContext';
import { api } from '@/services/api';
import { cores, espaco, etapasSolicitacao, iconeCategoria, raio, visualStatusSolicitacao, type NomeIcone } from '@/theme/tema';
import type { EventoSolicitacao, SolicitacaoDetalhada, StatusSolicitacao } from '@/types/dominio';
import { formatarData, formatarDataCompleta } from '@/utils/formatacao';

export default function TelaDetalheSolicitacao() {
  const sessao = useSessaoAtiva();
  const { id } = useLocalSearchParams<{ id: string }>();
  const [solicitacao, setSolicitacao] = useState<SolicitacaoDetalhada | null | undefined>();

  useEffect(() => {
    api.obterSolicitacao(sessao, id).then((s) => setSolicitacao(s ?? null));
  }, [sessao, id]);

  if (solicitacao === undefined) return <ActivityIndicator color={cores.azul} style={{ marginTop: 60 }} />;
  if (solicitacao === null) return <EstadoVazio icone="file-question-outline" titulo="Solicitação não encontrada" />;

  const status = visualStatusSolicitacao[solicitacao.status];
  const antigo = solicitacao.materialAntigo;
  const recusa = solicitacao.historico.find((e) => e.status === 'Recusada');

  return (
    <ScrollView contentContainerStyle={{ padding: espaco.l, gap: espaco.l, paddingBottom: 40 }}>
      <Cartao style={{ alignItems: 'center', paddingVertical: 24 }}>
        <IconeRedondo icone={status.icone} cor={status.cor} fundo={status.fundo} tamanho={56} />
        <Text style={estilos.protocolo}>{solicitacao.protocolo}</Text>
        <Text style={estilos.data}>Enviada em {formatarDataCompleta(solicitacao.criadaEm)}</Text>
        <View style={{ marginTop: 12 }}>
          <Chip rotulo={status.rotulo} cor={status.cor} fundo={status.fundo} icone={status.icone} />
        </View>
        <Text style={estilos.statusDescricao}>{status.descricao}</Text>
      </Cartao>

      {recusa?.comentario && <Aviso tipo="erro" texto={`Motivo da recusa: ${recusa.comentario}`} />}

      <Cartao>
        <TituloSecao titulo="Andamento" />
        <LinhaDoTempo status={solicitacao.status} historico={solicitacao.historico} />
      </Cartao>

      <Cartao>
        <TituloSecao titulo="EPI/EPC solicitado" />
        <View style={estilos.item}>
          <IconeRedondo icone={iconeCategoria(solicitacao.item.categoriaId)} cor={cores.azul} fundo={cores.azulSuave} tamanho={42} />
          <View style={{ flex: 1 }}>
            <Text style={estilos.itemNome}>{solicitacao.item.nome}</Text>
            <Text style={estilos.meta}>
              {solicitacao.item.codigo} · {solicitacao.item.categoria.nome}
            </Text>
          </View>
        </View>
        <Campo icone="counter" rotulo="Quantidade" valor={String(solicitacao.quantidade)} />
        {solicitacao.tamanho && <Campo icone="ruler" rotulo="Tamanho" valor={solicitacao.tamanho} />}
        <Campo icone="certificate-outline" rotulo="Número CA" valor={solicitacao.item.numeroCa ?? 'Não se aplica'} />
        <Campo icone="text-box-outline" rotulo="Motivo" valor={solicitacao.motivo.descricao} />
      </Cartao>

      <Cartao>
        <TituloSecao titulo={solicitacao.motivo.semDevolucao ? 'O que aconteceu' : 'Material antigo'} />
        {antigo.dataEntrega && <Campo icone="calendar-check-outline" rotulo="Entregue em" valor={formatarData(antigo.dataEntrega)} />}
        {antigo.fabricacao && <Campo icone="factory" rotulo="Fabricação" valor={antigo.fabricacao} />}
        {antigo.lote && <Campo icone="barcode" rotulo="Lote" valor={antigo.lote} />}
        {antigo.marca && <Campo icone="tag-outline" rotulo="Marca" valor={antigo.marca} />}
        <View style={estilos.relato}>
          <MaterialCommunityIcons name="format-quote-open" size={18} color={cores.inkMudo} />
          <Text style={estilos.relatoTexto}>{antigo.relato}</Text>
        </View>
      </Cartao>

      {solicitacao.fotos.length > 0 && (
        <Cartao>
          <TituloSecao titulo="Imagens do material" />
          <View style={{ flexDirection: 'row', gap: espaco.m }}>
            {solicitacao.fotos.map((uri) => (
              <Image key={uri} source={{ uri }} style={estilos.foto} resizeMode="cover" />
            ))}
          </View>
        </Cartao>
      )}

      <Cartao>
        <TituloSecao titulo="Assinatura do colaborador" />
        <AssinaturaSalva caminhos={solicitacao.assinatura} />
      </Cartao>
    </ScrollView>
  );
}

/** Etapas do fluxo com data/responsável de cada uma já concluída. */
function LinhaDoTempo({ status, historico }: { status: StatusSolicitacao; historico: EventoSolicitacao[] }) {
  // Uma solicitação recusada para na análise: mostra as etapas até ali e a recusa no lugar das seguintes.
  const etapas: StatusSolicitacao[] = status === 'Recusada' ? ['Pendente', 'EmAnalise', 'Recusada'] : etapasSolicitacao;

  return (
    <View>
      {etapas.map((etapa, i) => {
        const evento = historico.find((e) => e.status === etapa);
        const visual = visualStatusSolicitacao[etapa];
        const concluida = !!evento;
        const ultima = i === etapas.length - 1;
        return (
          <View key={etapa} style={{ flexDirection: 'row', gap: 12 }}>
            <View style={{ alignItems: 'center' }}>
              <View style={[estilos.marcador, concluida ? { backgroundColor: visual.cor, borderColor: visual.cor } : null]}>
                {concluida && <MaterialCommunityIcons name={etapa === 'Recusada' ? 'close' : 'check'} size={13} color="#FFFFFF" />}
              </View>
              {!ultima && <View style={[estilos.trilha, concluida && historico.some((e) => e.status === etapas[i + 1]) && { backgroundColor: visual.cor }]} />}
            </View>
            <View style={{ flex: 1, paddingBottom: ultima ? 0 : 18 }}>
              <Text style={[estilos.etapaTitulo, !concluida && { color: cores.inkMudo }]}>{visual.rotulo}</Text>
              {evento ? (
                <>
                  <Text style={estilos.etapaMeta}>
                    {formatarDataCompleta(evento.dataHora)} · {evento.responsavel}
                  </Text>
                  {evento.comentario && etapa !== 'Recusada' && <Text style={estilos.etapaComentario}>{evento.comentario}</Text>}
                </>
              ) : (
                <Text style={estilos.etapaMeta}>Aguardando</Text>
              )}
            </View>
          </View>
        );
      })}
    </View>
  );
}

function Campo({ icone, rotulo, valor }: { icone: NomeIcone; rotulo: string; valor: string }) {
  return (
    <View style={estilos.campo}>
      <MaterialCommunityIcons name={icone} size={18} color={cores.inkMudo} style={{ marginTop: 1 }} />
      <Text style={estilos.campoRotulo}>{rotulo}</Text>
      <Text style={estilos.campoValor}>{valor}</Text>
    </View>
  );
}

const estilos = StyleSheet.create({
  protocolo: { fontSize: 20, fontWeight: '800', color: cores.ink, marginTop: 12, letterSpacing: 0.3 },
  data: { fontSize: 13.5, color: cores.inkSuave, marginTop: 2 },
  statusDescricao: { fontSize: 13, color: cores.inkSuave, marginTop: 8 },
  item: { flexDirection: 'row', alignItems: 'center', gap: 12, padding: 10, borderRadius: raio.m, backgroundColor: cores.paper, marginBottom: 6 },
  itemNome: { fontSize: 15, fontWeight: '700', color: cores.ink },
  meta: { fontSize: 12.5, color: cores.inkSuave, marginTop: 1 },
  campo: { flexDirection: 'row', alignItems: 'flex-start', gap: 10, paddingVertical: 6 },
  campoRotulo: { width: 100, fontSize: 13.5, color: cores.inkSuave },
  campoValor: { flex: 1, fontSize: 14, fontWeight: '600', color: cores.ink },
  relato: { flexDirection: 'row', gap: 8, marginTop: 8, padding: 12, borderRadius: raio.m, backgroundColor: cores.paper },
  relatoTexto: { flex: 1, fontSize: 14, color: cores.ink, lineHeight: 20 },
  foto: { width: 120, height: 120, borderRadius: raio.m, backgroundColor: cores.paper },
  marcador: { width: 22, height: 22, borderRadius: 11, borderWidth: 2, borderColor: cores.linha, backgroundColor: cores.surface, alignItems: 'center', justifyContent: 'center' },
  trilha: { flex: 1, width: 2, backgroundColor: cores.linha, marginVertical: 2 },
  etapaTitulo: { fontSize: 14.5, fontWeight: '700', color: cores.ink },
  etapaMeta: { fontSize: 12.5, color: cores.inkSuave, marginTop: 2 },
  etapaComentario: { fontSize: 12.5, color: cores.ink, marginTop: 4, fontStyle: 'italic' },
});
