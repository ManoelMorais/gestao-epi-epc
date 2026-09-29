import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { useFocusEffect } from 'expo-router';
import { useCallback, useState } from 'react';
import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Avatar, Aviso, Botao, Cartao, Chip, IconeRedondo, TituloSecao } from '@/components/ui';
import { useSessao, useSessaoAtiva } from '@/context/SessaoContext';
import { api } from '@/services/api';
import { cores, espaco, iconeCategoria, visualStatusColaborador, type NomeIcone } from '@/theme/tema';
import type { ItemDetalhado } from '@/types/dominio';
import { formatarData } from '@/utils/formatacao';

export default function TelaPerfil() {
  const { colaborador } = useSessaoAtiva();
  const sessao = useSessaoAtiva();
  const { sair } = useSessao();
  const insets = useSafeAreaInsets();
  const status = visualStatusColaborador[colaborador.status];

  const [liberados, setLiberados] = useState<ItemDetalhado[] | null>(null);
  useFocusEffect(
    useCallback(() => {
      let cancelado = false;
      api.listarItensElegiveis(sessao).then((lista) => !cancelado && setLiberados(lista));
      return () => {
        cancelado = true;
      };
    }, [sessao]),
  );

  return (
    <ScrollView contentContainerStyle={{ paddingTop: insets.top + espaco.l, paddingHorizontal: espaco.l, paddingBottom: 32, gap: espaco.l }}>
      <Text style={estilos.titulo}>Meu cadastro</Text>

      <Cartao style={{ alignItems: 'center', paddingVertical: 24, gap: 6 }}>
        <Avatar nome={colaborador.nome} tamanho={76} />
        <Text style={estilos.nome}>{colaborador.nome}</Text>
        <Text style={estilos.meta}>DRT {colaborador.drt}</Text>
        <View style={{ alignItems: 'center' }}>
          <Chip rotulo={status.rotulo} cor={status.cor} fundo={status.fundo} icone={status.icone} />
        </View>
      </Cartao>

      <Cartao>
        <TituloSecao titulo="Dados funcionais" />
        <Campo icone="briefcase-outline" rotulo="Cargo" valor={colaborador.cargo.nome} />
        <Campo icone="sitemap-outline" rotulo="Área" valor={colaborador.area} />
        <Campo icone="office-building-outline" rotulo="Unidade" valor={`${colaborador.unidade.nome} (${colaborador.unidade.sigla})`} />
        <Campo icone="map-marker-outline" rotulo="Cidade" valor={colaborador.unidade.cidade} />
        <Campo icone="calendar-account-outline" rotulo="Admissão" valor={`${formatarData(colaborador.admissao)} · ${tempoDeCasa(colaborador.admissao)}`} />
      </Cartao>

      <Cartao>
        <TituloSecao titulo="Contato" />
        <Campo icone="email-outline" rotulo="E-mail" valor={colaborador.email} />
        <Campo icone="phone-outline" rotulo="Telefone" valor={colaborador.telefone} />
      </Cartao>

      <Cartao>
        <TituloSecao titulo="EPI/EPC liberados para o seu cargo" />
        {liberados?.length === 0 ? (
          <Text style={estilos.meta}>Seu cargo não exige EPI/EPC de campo.</Text>
        ) : (
          <View style={{ gap: 10 }}>
            {(liberados ?? []).map((item) => (
              <View key={item.id} style={{ flexDirection: 'row', alignItems: 'center', gap: 10 }}>
                <IconeRedondo icone={iconeCategoria(item.categoriaId)} cor={cores.azul} fundo={cores.azulSuave} tamanho={30} />
                <Text style={estilos.item} numberOfLines={1}>
                  {item.nome}
                </Text>
                <Text style={estilos.codigo}>{item.codigo}</Text>
              </View>
            ))}
          </View>
        )}
      </Cartao>

      <Aviso tipo="info" texto="Encontrou algum dado errado? Procure o RH da sua unidade — o cadastro é mantido pelo sistema de gestão." />

      <Botao titulo="Sair da conta" icone="logout" variante="secundario" onPress={sair} />
    </ScrollView>
  );
}

function tempoDeCasa(admissaoIso: string) {
  const meses = Math.floor((Date.now() - new Date(admissaoIso).getTime()) / (30.44 * 86_400_000));
  const anos = Math.floor(meses / 12);
  if (anos >= 1) return `${anos} ${anos === 1 ? 'ano' : 'anos'} de empresa`;
  return `${meses} ${meses === 1 ? 'mês' : 'meses'} de empresa`;
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
  titulo: { fontSize: 28, fontWeight: '800', color: cores.ink, letterSpacing: -0.5 },
  nome: { fontSize: 19, fontWeight: '800', color: cores.ink, marginTop: 8, textAlign: 'center' },
  meta: { fontSize: 13.5, color: cores.inkSuave },
  campo: { flexDirection: 'row', alignItems: 'flex-start', gap: 10, paddingVertical: 7 },
  campoRotulo: { width: 86, fontSize: 13.5, color: cores.inkSuave },
  campoValor: { flex: 1, fontSize: 14, fontWeight: '600', color: cores.ink },
  item: { flex: 1, fontSize: 14, fontWeight: '600', color: cores.ink },
  codigo: { fontSize: 12, color: cores.inkMudo, fontWeight: '600' },
});
