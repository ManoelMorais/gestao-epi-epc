import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import * as ImagePicker from 'expo-image-picker';
import { LinearGradient } from 'expo-linear-gradient';
import { router, useFocusEffect, useLocalSearchParams } from 'expo-router';
import { useCallback, useMemo, useState, type ReactNode } from 'react';
import { ActivityIndicator, FlatList, Image, KeyboardAvoidingView, Modal, Platform, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { PainelAssinatura } from '@/components/Assinatura';
import { Avatar, Aviso, Botao, CampoTexto, Cartao, Chip, ErroCarregamento, EstadoVazio, IconeRedondo } from '@/components/ui';
import { useSessaoAtiva } from '@/context/SessaoContext';
import { tamanhosSugeridos } from '@/data/seed';
import { api, ErroNegocio, mensagemDeErro } from '@/services/api';
import { cores, espaco, gradienteMarca, iconeCategoria, raio, sombra, visualValidade, type NomeIcone } from '@/theme/tema';
import type { ItemDetalhado, ItemEmPosse, MotivoMovimentacao, Sessao, SolicitacaoDetalhada } from '@/types/dominio';
import { formatarData, formatarDataCompleta, lerData, mascaraData, mascaraMesAno, mesAnoValido } from '@/utils/formatacao';

const MAX_FOTOS = 2;

interface Dados {
  elegiveis: ItemDetalhado[];
  motivos: MotivoMovimentacao[];
  emPosse: ItemEmPosse[];
}

export default function TelaNovaSolicitacao() {
  const sessao = useSessaoAtiva();
  const params = useLocalSearchParams<{ item?: string }>();
  const [dados, setDados] = useState<Dados | null>(null);
  const [enviada, setEnviada] = useState<SolicitacaoDetalhada | null>(null);
  const [rodada, setRodada] = useState(0);
  const [erroCarga, setErroCarga] = useState<string | null>(null);

  const carregar = useCallback(async () => {
    try {
      const [elegiveis, motivos, emPosse] = await Promise.all([api.listarItensElegiveis(sessao), api.listarMotivos(), api.listarItensEmPosse(sessao)]);
      setDados({ elegiveis, motivos, emPosse });
      setErroCarga(null);
    } catch (e) {
      setErroCarga(mensagemDeErro(e));
    }
  }, [sessao]);

  // Recarrega catálogo e itens em posse sempre que a aba ganha foco.
  useFocusEffect(
    useCallback(() => {
      carregar();
    }, [carregar]),
  );

  function novaSolicitacao() {
    setEnviada(null);
    setRodada((r) => r + 1);
    router.setParams({ item: undefined });
  }

  if (enviada) return <Sucesso solicitacao={enviada} onNova={novaSolicitacao} />;
  if (!dados && erroCarga) {
    return (
      <View style={{ flex: 1, justifyContent: 'center', paddingHorizontal: espaco.l }}>
        <ErroCarregamento mensagem={erroCarga} onTentar={carregar} />
      </View>
    );
  }
  if (!dados) return <ActivityIndicator color={cores.azul} style={{ marginTop: 120 }} />;

  // A chave remonta o formulário quando chega um item novo pela URL (ex.: tocar num
  // EPI no dashboard) ou depois de enviar — sempre começa limpo.
  return <Formulario key={`${params.item ?? 'livre'}-${rodada}`} sessao={sessao} dados={dados} itemInicial={params.item} onEnviada={setEnviada} />;
}

function Formulario({ sessao, dados, itemInicial, onEnviada }: { sessao: Sessao; dados: Dados; itemInicial?: string; onEnviada: (s: SolicitacaoDetalhada) => void }) {
  const insets = useSafeAreaInsets();
  const { colaborador } = sessao;
  const posseInicial = dados.emPosse.find((p) => p.item.id === itemInicial);

  // ---- Identificação do EPI/EPC -------------------------------------------
  const [itemId, setItemId] = useState<string | undefined>(dados.elegiveis.some((i) => i.id === itemInicial) ? itemInicial : undefined);
  const [tamanho, setTamanho] = useState<string | undefined>(posseInicial?.tamanho);
  const [motivoId, setMotivoId] = useState<string | undefined>(
    posseInicial?.diasParaVencer !== undefined && posseInicial.diasParaVencer <= 0 ? 'mot-validade' : undefined,
  );
  const [quantidade, setQuantidade] = useState(posseInicial?.quantidade ?? 1);

  // ---- Material antigo ------------------------------------------------------
  const [dataEntrega, setDataEntrega] = useState(posseInicial ? formatarData(posseInicial.recebidoEm) : '');
  const [fabricacao, setFabricacao] = useState('');
  const [marca, setMarca] = useState('');
  const [lote, setLote] = useState('');
  const [relato, setRelato] = useState('');

  // ---- Evidências e assinatura ------------------------------------------------
  const [fotos, setFotos] = useState<string[]>([]);
  const [assinatura, setAssinatura] = useState('');
  const [desenhando, setDesenhando] = useState(false);

  const [seletorAberto, setSeletorAberto] = useState(false);
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  const item = dados.elegiveis.find((i) => i.id === itemId);
  const posse = dados.emPosse.find((p) => p.item.id === itemId);
  const motivo = dados.motivos.find((m) => m.id === motivoId);
  const semDevolucao = !!motivo?.semDevolucao;

  function escolherItem(novo: ItemDetalhado) {
    const p = dados.emPosse.find((x) => x.item.id === novo.id);
    setItemId(novo.id);
    // Pré-preenche com o que o sistema já sabe sobre o item que está com o colaborador.
    setTamanho(p?.tamanho);
    setQuantidade(p?.quantidade ?? 1);
    setDataEntrega(p ? formatarData(p.recebidoEm) : '');
    if (p?.diasParaVencer !== undefined && p.diasParaVencer <= 0) setMotivoId('mot-validade');
    setSeletorAberto(false);
    setErro(null);
  }

  // Lista do que ainda falta, mostrada no rodapé — mais útil que só desabilitar o botão.
  const pendencias = useMemo(() => {
    const p: string[] = [];
    if (!item) p.push('item');
    if (item?.possuiTamanho && !tamanho) p.push('tamanho');
    if (!motivo) p.push('motivo');
    if (motivo && !semDevolucao) {
      if (!lerData(dataEntrega)) p.push('data de entrega');
      if (!mesAnoValido(fabricacao)) p.push('fabricação');
      if (!lote.trim()) p.push('lote');
    }
    if (relato.trim().length < 10) p.push('relato');
    if (!assinatura) p.push('assinatura');
    return p;
  }, [item, tamanho, motivo, semDevolucao, dataEntrega, fabricacao, lote, relato, assinatura]);

  async function adicionarFoto(origem: 'camera' | 'galeria') {
    setErro(null);
    const permissao = origem === 'camera' ? await ImagePicker.requestCameraPermissionsAsync() : await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (!permissao.granted) {
      setErro(origem === 'camera' ? 'Permita o acesso à câmera nas configurações do celular para fotografar o item.' : 'Permita o acesso às fotos nas configurações do celular.');
      return;
    }
    const abrir = origem === 'camera' ? ImagePicker.launchCameraAsync : ImagePicker.launchImageLibraryAsync;
    const resultado = await abrir({ mediaTypes: ['images'], quality: 0.6, allowsEditing: true, aspect: [4, 3] });
    if (!resultado.canceled && resultado.assets[0]) {
      setFotos((atual) => [...atual, resultado.assets[0].uri].slice(0, MAX_FOTOS));
    }
  }

  async function enviar() {
    if (pendencias.length > 0 || !item || !motivo) return;
    setErro(null);
    setEnviando(true);
    try {
      const solicitacao = await api.criarSolicitacao(sessao, {
        itemId: item.id,
        tamanho: item.possuiTamanho ? tamanho : undefined,
        quantidade,
        motivoId: motivo.id,
        materialAntigo: semDevolucao
          ? { relato }
          : { relato, lote: lote.trim(), marca: marca.trim() || undefined, fabricacao, dataEntrega: lerData(dataEntrega)!.toISOString() },
        fotos,
        assinatura,
      });
      onEnviada(solicitacao);
    } catch (e) {
      setErro(e instanceof ErroNegocio ? e.message : 'Não foi possível enviar. Tente novamente.');
    } finally {
      setEnviando(false);
    }
  }

  if (dados.elegiveis.length === 0) {
    return (
      <View style={{ flex: 1, paddingTop: insets.top + 40, paddingHorizontal: espaco.l }}>
        <EstadoVazio
          icone="shield-off-outline"
          titulo="Nenhum EPI/EPC liberado para o seu cargo"
          descricao={`O cargo ${colaborador.cargo.nome} não tem itens de proteção cadastrados. Se precisar de algum, fale com a Segurança do Trabalho.`}
        />
      </View>
    );
  }

  return (
    <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView
        scrollEnabled={!desenhando}
        contentContainerStyle={{ paddingTop: insets.top + espaco.l, paddingHorizontal: espaco.l, paddingBottom: 24, gap: espaco.l }}
        keyboardShouldPersistTaps="handled"
      >
        <View>
          <Text style={estilos.titulo}>Solicitar troca</Text>
          <Text style={estilos.subtitulo}>Preencha os dados do EPI/EPC que precisa ser substituído.</Text>
        </View>

        {/* Identificação do colaborador — automática, igual ao topo do formulário do SIGME */}
        <LinearGradient colors={gradienteMarca} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }} style={estilos.identificacao}>
          <Text style={estilos.identificacaoTitulo}>IDENTIFICAÇÃO DO COLABORADOR</Text>
          <View style={{ flexDirection: 'row', alignItems: 'center', gap: 12 }}>
            <Avatar nome={colaborador.nome} tamanho={52} />
            <View style={{ flex: 1, gap: 2 }}>
              <Text style={estilos.identificacaoNome}>{colaborador.nome}</Text>
              <Text style={estilos.identificacaoMeta}>DRT {colaborador.drt} · {colaborador.cargo.nome}</Text>
              <Text style={estilos.identificacaoMeta}>
                {colaborador.area} · {colaborador.unidade.sigla} · {colaborador.unidade.cidade}
              </Text>
            </View>
          </View>
        </LinearGradient>

        {colaborador.status === 'Afastado' && <Aviso tipo="alerta" texto="Você está afastado: a solicitação será enviada, mas só será analisada no seu retorno." />}

        {/* Identificação do EPI/EPC */}
        <Secao titulo="Identificação do EPI/EPC" icone="shield-half-full">
          <Rotulo texto="Descrição do item" obrigatorio />
          <Pressable onPress={() => setSeletorAberto(true)} style={[estilos.seletor, item && { borderColor: cores.azul }]}>
            {item ? (
              <>
                <IconeRedondo icone={iconeCategoria(item.categoriaId)} cor={cores.azul} fundo={cores.azulSuave} tamanho={36} />
                <View style={{ flex: 1 }}>
                  <Text style={estilos.seletorTexto} numberOfLines={2}>
                    {item.nome}
                  </Text>
                  <Text style={estilos.seletorMeta}>{item.codigo} · {item.categoria.nome}</Text>
                </View>
              </>
            ) : (
              <Text style={[estilos.seletorTexto, { flex: 1, color: cores.inkMudo, fontWeight: '500' }]}>Selecione o EPI/EPC</Text>
            )}
            <MaterialCommunityIcons name="chevron-down" size={22} color={cores.inkMudo} />
          </Pressable>
          {posse && (
            <View style={{ flexDirection: 'row', alignItems: 'center', gap: 8 }}>
              <Chip {...visualValidade(posse)} rotulo={visualValidade(posse).rotulo} />
              <Text style={estilos.dica}>Com você desde {formatarData(posse.recebidoEm)}</Text>
            </View>
          )}

          {item?.possuiTamanho && (
            <>
              <Rotulo texto="Tamanho" obrigatorio />
              <View style={estilos.linhaOpcoes}>
                {tamanhosSugeridos(item).map((t) => (
                  <Opcao key={t} rotulo={t} ativo={tamanho === t} onPress={() => setTamanho(t)} compacta />
                ))}
              </View>
            </>
          )}

          <Rotulo texto="Motivo da saída" obrigatorio />
          <View style={estilos.linhaOpcoes}>
            {dados.motivos.map((m) => (
              <Opcao key={m.id} rotulo={m.descricao} ativo={motivoId === m.id} onPress={() => setMotivoId(m.id)} />
            ))}
          </View>

          <View style={{ flexDirection: 'row', gap: espaco.m }}>
            <View style={{ flex: 1, gap: 6 }}>
              <Rotulo texto="Quantidade" obrigatorio />
              <View style={estilos.stepper}>
                <Pressable onPress={() => setQuantidade((q) => Math.max(1, q - 1))} style={estilos.stepperBotao} accessibilityLabel="Diminuir quantidade">
                  <MaterialCommunityIcons name="minus" size={18} color={quantidade <= 1 ? cores.linha : cores.azul} />
                </Pressable>
                <Text style={estilos.stepperValor}>{quantidade}</Text>
                <Pressable onPress={() => setQuantidade((q) => Math.min(10, q + 1))} style={estilos.stepperBotao} accessibilityLabel="Aumentar quantidade">
                  <MaterialCommunityIcons name="plus" size={18} color={cores.azul} />
                </Pressable>
              </View>
            </View>
            <View style={{ flex: 1, gap: 6 }}>
              <Rotulo texto="Número CA" />
              <View style={[estilos.campoSomenteLeitura]}>
                <MaterialCommunityIcons name="certificate-outline" size={18} color={cores.inkMudo} />
                <Text style={estilos.somenteLeituraTexto}>{item ? item.numeroCa ?? 'Não se aplica' : '—'}</Text>
              </View>
            </View>
          </View>
        </Secao>

        {/* Material antigo */}
        <Secao titulo={semDevolucao ? 'O que aconteceu' : 'Material antigo'} icone="package-variant">
          {semDevolucao ? (
            <Aviso tipo="info" texto="Em caso de perda não há item para devolver. Conte como aconteceu — a Segurança do Trabalho pode entrar em contato." />
          ) : (
            <>
              <View style={{ flexDirection: 'row', gap: espaco.m }}>
                <View style={{ flex: 1 }}>
                  <CampoTexto
                    rotulo="Data de entrega *"
                    placeholder="dd/mm/aaaa"
                    keyboardType="number-pad"
                    value={dataEntrega}
                    onChangeText={(t) => setDataEntrega(mascaraData(t))}
                    maxLength={10}
                    erro={dataEntrega.length === 10 && !lerData(dataEntrega)}
                  />
                </View>
                <View style={{ flex: 1 }}>
                  <CampoTexto
                    rotulo="Fabricação *"
                    placeholder="mm/aaaa"
                    keyboardType="number-pad"
                    value={fabricacao}
                    onChangeText={(t) => setFabricacao(mascaraMesAno(t))}
                    maxLength={7}
                    erro={fabricacao.length === 7 && !mesAnoValido(fabricacao)}
                  />
                </View>
              </View>
              <CampoTexto rotulo="Número do lote *" placeholder="Etiqueta do item (ex.: LT-2291)" autoCapitalize="characters" value={lote} onChangeText={setLote} maxLength={30} />
              <CampoTexto
                rotulo={item?.categoriaId === 'cat-vestimenta' ? 'Marca do uniforme devolvido' : 'Marca do item devolvido'}
                placeholder="Opcional"
                value={marca}
                onChangeText={setMarca}
                maxLength={40}
              />
            </>
          )}
          <Rotulo texto={semDevolucao ? 'Descreva como o item foi perdido' : 'Descreva o que aconteceu com o material antigo'} obrigatorio />
          <TextInput
            style={estilos.areaTexto}
            placeholder={semDevolucao ? 'Onde e quando aconteceu, e se foi registrado ocorrência.' : 'Ex.: costura rasgou na manga durante manutenção em poste.'}
            placeholderTextColor={cores.inkMudo}
            value={relato}
            onChangeText={setRelato}
            multiline
            maxLength={400}
            textAlignVertical="top"
          />
          <Text style={estilos.contador}>{relato.trim().length < 10 ? `Mínimo de 10 caracteres` : `${relato.length}/400`}</Text>
        </Secao>

        {/* Fotos */}
        <Secao titulo="Imagens do material" icone="camera-outline" opcional>
          <Text style={estilos.dica}>Fotografe o item {semDevolucao ? 'ou o local, se ajudar' : 'mostrando o dano, o desgaste ou a etiqueta'}. Até {MAX_FOTOS} fotos.</Text>
          <View style={{ flexDirection: 'row', gap: espaco.m }}>
            {fotos.map((uri, i) => (
              <View key={uri} style={estilos.foto}>
                <Image source={{ uri }} style={StyleSheet.absoluteFill} resizeMode="cover" />
                <Pressable onPress={() => setFotos((f) => f.filter((_, j) => j !== i))} style={estilos.removerFoto} hitSlop={8} accessibilityLabel="Remover foto">
                  <MaterialCommunityIcons name="close" size={16} color="#FFFFFF" />
                </Pressable>
              </View>
            ))}
            {fotos.length < MAX_FOTOS && (
              <View style={[estilos.foto, estilos.fotoVazia]}>
                <Pressable onPress={() => adicionarFoto('camera')} style={estilos.botaoFoto}>
                  <MaterialCommunityIcons name="camera-plus-outline" size={22} color={cores.azul} />
                  <Text style={estilos.botaoFotoTexto}>Câmera</Text>
                </Pressable>
                <View style={{ height: 1, alignSelf: 'stretch', backgroundColor: cores.linha }} />
                <Pressable onPress={() => adicionarFoto('galeria')} style={estilos.botaoFoto}>
                  <MaterialCommunityIcons name="image-plus" size={22} color={cores.azul} />
                  <Text style={estilos.botaoFotoTexto}>Galeria</Text>
                </Pressable>
              </View>
            )}
          </View>
        </Secao>

        {/* Assinatura */}
        <Secao titulo="Assinatura do colaborador" icone="draw-pen">
          <Text style={estilos.dica}>Ao assinar, você confirma que as informações acima são verdadeiras.</Text>
          <PainelAssinatura valor={assinatura} onChange={setAssinatura} onDesenhando={setDesenhando} />
        </Secao>

      </ScrollView>

      {/* O erro fica colado ao botão Enviar: no fim do formulário ele poderia ficar fora da tela. */}
      {erro && (
        <View style={estilos.erroRodape}>
          <Aviso tipo="erro" texto={erro} />
        </View>
      )}
      <View style={estilos.rodape}>
        <View style={{ flex: 1 }}>
          {pendencias.length === 0 ? (
            <>
              <Text style={estilos.rodapeRotulo}>Tudo pronto</Text>
              <Text style={estilos.rodapeValor} numberOfLines={1}>
                {quantidade}× {item?.nome}
              </Text>
            </>
          ) : (
            <>
              <Text style={estilos.rodapeRotulo}>Falta preencher</Text>
              <Text style={[estilos.rodapeValor, { color: cores.laranja, fontSize: 13.5 }]} numberOfLines={2}>
                {pendencias.join(', ')}
              </Text>
            </>
          )}
        </View>
        <Botao titulo="Enviar" icone="send" onPress={enviar} carregando={enviando} desabilitado={pendencias.length > 0} style={{ minWidth: 130 }} />
      </View>

      <SeletorItem
        visivel={seletorAberto}
        itens={dados.elegiveis}
        emPosse={dados.emPosse}
        selecionado={itemId}
        onEscolher={escolherItem}
        onFechar={() => setSeletorAberto(false)}
      />
    </KeyboardAvoidingView>
  );
}

function Secao({ titulo, icone, opcional, children }: { titulo: string; icone: NomeIcone; opcional?: boolean; children: ReactNode }) {
  return (
    <Cartao style={{ gap: 12 }}>
      <View style={estilos.secaoCabecalho}>
        <IconeRedondo icone={icone} cor={cores.azul} fundo={cores.azulSuave} tamanho={30} />
        <Text style={estilos.secaoTitulo}>{titulo}</Text>
        {opcional && <Text style={estilos.opcional}>Opcional</Text>}
      </View>
      {children}
    </Cartao>
  );
}

function Rotulo({ texto, obrigatorio }: { texto: string; obrigatorio?: boolean }) {
  return (
    <Text style={estilos.rotulo}>
      {texto}
      {obrigatorio && <Text style={{ color: cores.vermelho }}> *</Text>}
    </Text>
  );
}

function Opcao({ rotulo, ativo, onPress, compacta }: { rotulo: string; ativo: boolean; onPress: () => void; compacta?: boolean }) {
  return (
    <Pressable
      onPress={onPress}
      style={[estilos.opcao, compacta && estilos.opcaoCompacta, ativo && estilos.opcaoAtiva]}
      accessibilityRole="radio"
      accessibilityState={{ selected: ativo }}
    >
      {ativo && !compacta && <MaterialCommunityIcons name="check" size={16} color="#FFFFFF" />}
      <Text style={[estilos.opcaoTexto, ativo && { color: '#FFFFFF' }]}>{rotulo}</Text>
    </Pressable>
  );
}

function SeletorItem({
  visivel,
  itens,
  emPosse,
  selecionado,
  onEscolher,
  onFechar,
}: {
  visivel: boolean;
  itens: ItemDetalhado[];
  emPosse: ItemEmPosse[];
  selecionado?: string;
  onEscolher: (item: ItemDetalhado) => void;
  onFechar: () => void;
}) {
  const insets = useSafeAreaInsets();
  // Itens que já estão com o colaborador aparecem primeiro — são os candidatos naturais a troca.
  const ordenados = [...itens].sort((a, b) => Number(emPosse.some((p) => p.item.id === b.id)) - Number(emPosse.some((p) => p.item.id === a.id)));

  return (
    <Modal visible={visivel} animationType="slide" transparent onRequestClose={onFechar}>
      <Pressable style={estilos.fundoModal} onPress={onFechar} />
      <View style={[estilos.folha, { paddingBottom: insets.bottom + 12 }]}>
        <View style={estilos.alca} />
        <View style={estilos.folhaCabecalho}>
          <View style={{ flex: 1 }}>
            <Text style={estilos.folhaTitulo}>Escolha o EPI/EPC</Text>
            <Text style={estilos.dica}>Somente itens liberados para o seu cargo</Text>
          </View>
          <Pressable onPress={onFechar} hitSlop={10} accessibilityLabel="Fechar">
            <MaterialCommunityIcons name="close" size={24} color={cores.inkSuave} />
          </Pressable>
        </View>
        <FlatList
          data={ordenados}
          keyExtractor={(i) => i.id}
          ItemSeparatorComponent={() => <View style={{ height: 8 }} />}
          renderItem={({ item }) => {
            const posse = emPosse.find((p) => p.item.id === item.id);
            const ativo = item.id === selecionado;
            return (
              <Pressable onPress={() => onEscolher(item)} style={[estilos.opcaoItem, ativo && { borderColor: cores.azul, backgroundColor: '#F8FAFF' }]}>
                <IconeRedondo icone={iconeCategoria(item.categoriaId)} cor={cores.azul} fundo={cores.azulSuave} tamanho={38} />
                <View style={{ flex: 1, gap: 3 }}>
                  <Text style={estilos.seletorTexto}>{item.nome}</Text>
                  <Text style={estilos.seletorMeta}>
                    {item.codigo}
                    {item.numeroCa ? ` · CA ${item.numeroCa}` : ''} · {item.categoria.tipo.toUpperCase()}
                  </Text>
                  {posse && <Chip {...visualValidade(posse)} rotulo={`Com você · ${visualValidade(posse).rotulo}`} />}
                </View>
                {ativo && <MaterialCommunityIcons name="check-circle" size={22} color={cores.azul} />}
              </Pressable>
            );
          }}
          contentContainerStyle={{ padding: espaco.l, paddingTop: 4 }}
        />
      </View>
    </Modal>
  );
}

function Sucesso({ solicitacao, onNova }: { solicitacao: SolicitacaoDetalhada; onNova: () => void }) {
  const insets = useSafeAreaInsets();
  return (
    <ScrollView contentContainerStyle={[estilos.sucesso, { paddingTop: insets.top + 48 }]}>
      <View style={estilos.sucessoIcone}>
        <MaterialCommunityIcons name="send-check" size={42} color="#FFFFFF" />
      </View>
      <Text style={estilos.sucessoTitulo}>Solicitação enviada!</Text>
      <Text style={estilos.sucessoTexto}>O almoxarifado e a Segurança do Trabalho vão analisar seu pedido. Você acompanha tudo em “Solicitações”.</Text>

      <Cartao style={{ alignSelf: 'stretch', marginTop: 28, gap: 14 }}>
        <LinhaResumo rotulo="Protocolo" valor={solicitacao.protocolo} destaque />
        <View style={estilos.divisor} />
        <LinhaResumo rotulo="Item" valor={solicitacao.item.nome} />
        <LinhaResumo rotulo="Quantidade" valor={`${solicitacao.quantidade}${solicitacao.tamanho ? ` · Tam. ${solicitacao.tamanho}` : ''}`} />
        <LinhaResumo rotulo="Motivo" valor={solicitacao.motivo.descricao} />
        <LinhaResumo rotulo="Enviada em" valor={formatarDataCompleta(solicitacao.criadaEm)} />
      </Cartao>

      <View style={{ alignSelf: 'stretch', gap: espaco.m, marginTop: 24 }}>
        <Botao titulo="Acompanhar solicitação" icone="eye-outline" onPress={() => router.push({ pathname: '/solicitacao/[id]', params: { id: solicitacao.id } })} />
        <Botao titulo="Nova solicitação" variante="secundario" onPress={onNova} />
      </View>
    </ScrollView>
  );
}

function LinhaResumo({ rotulo, valor, destaque }: { rotulo: string; valor: string; destaque?: boolean }) {
  return (
    <View style={estilos.linhaResumo}>
      <Text style={estilos.resumoRotulo}>{rotulo}</Text>
      <Text style={[estilos.resumoValor, destaque && estilos.resumoDestaque]}>{valor}</Text>
    </View>
  );
}

const estilos = StyleSheet.create({
  titulo: { fontSize: 28, fontWeight: '800', color: cores.ink, letterSpacing: -0.5 },
  subtitulo: { fontSize: 14, color: cores.inkSuave, marginTop: 2 },
  identificacao: { borderRadius: raio.g, padding: espaco.l, gap: 12, ...sombra, shadowColor: cores.azul, shadowOpacity: 0.25 },
  identificacaoTitulo: { color: 'rgba(255,255,255,0.75)', fontSize: 11.5, fontWeight: '800', letterSpacing: 1 },
  identificacaoNome: { color: '#FFFFFF', fontSize: 16.5, fontWeight: '800' },
  identificacaoMeta: { color: 'rgba(255,255,255,0.85)', fontSize: 12.5 },
  secaoCabecalho: { flexDirection: 'row', alignItems: 'center', gap: 10, marginBottom: 2 },
  secaoTitulo: { flex: 1, fontSize: 16, fontWeight: '700', color: cores.ink },
  opcional: { fontSize: 12, color: cores.inkMudo, fontWeight: '600' },
  rotulo: { fontSize: 13, fontWeight: '600', color: cores.inkSuave, marginTop: 4 },
  dica: { fontSize: 12.5, color: cores.inkSuave, lineHeight: 18 },
  seletor: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
    minHeight: 56,
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderRadius: raio.m,
    borderWidth: 1.5,
    borderColor: cores.linha,
    backgroundColor: cores.surface,
  },
  seletorTexto: { fontSize: 14.5, fontWeight: '700', color: cores.ink },
  seletorMeta: { fontSize: 12, color: cores.inkMudo, marginTop: 1 },
  linhaOpcoes: { flexDirection: 'row', flexWrap: 'wrap', gap: 8 },
  opcao: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    paddingHorizontal: 14,
    height: 38,
    borderRadius: 19,
    borderWidth: 1,
    borderColor: cores.linha,
    backgroundColor: cores.surface,
  },
  opcaoCompacta: { minWidth: 44, height: 36, paddingHorizontal: 10, borderRadius: 10, justifyContent: 'center' },
  opcaoAtiva: { backgroundColor: cores.azul, borderColor: cores.azul },
  opcaoTexto: { fontSize: 13.5, fontWeight: '600', color: cores.inkSuave },
  stepper: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', height: 52, borderRadius: raio.m, borderWidth: 1.5, borderColor: cores.linha, backgroundColor: cores.surface },
  stepperBotao: { width: 44, height: '100%', alignItems: 'center', justifyContent: 'center' },
  stepperValor: { fontSize: 17, fontWeight: '800', color: cores.ink },
  campoSomenteLeitura: { flexDirection: 'row', alignItems: 'center', gap: 8, height: 52, paddingHorizontal: 14, borderRadius: raio.m, backgroundColor: cores.paper },
  somenteLeituraTexto: { fontSize: 15, fontWeight: '700', color: cores.inkSuave },
  areaTexto: { minHeight: 96, borderRadius: raio.m, borderWidth: 1.5, borderColor: cores.linha, padding: 12, fontSize: 15, color: cores.ink },
  contador: { fontSize: 11.5, color: cores.inkMudo, textAlign: 'right', marginTop: -6 },
  foto: { width: 108, height: 108, borderRadius: raio.m, overflow: 'hidden', backgroundColor: cores.paper },
  fotoVazia: { borderWidth: 1.5, borderStyle: 'dashed', borderColor: cores.linha, alignItems: 'center' },
  botaoFoto: { flex: 1, alignSelf: 'stretch', alignItems: 'center', justifyContent: 'center', flexDirection: 'row', gap: 6 },
  botaoFotoTexto: { fontSize: 12.5, fontWeight: '700', color: cores.azul },
  removerFoto: { position: 'absolute', top: 6, right: 6, width: 24, height: 24, borderRadius: 12, backgroundColor: 'rgba(15,23,32,0.7)', alignItems: 'center', justifyContent: 'center' },
  rodape: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: espaco.m,
    paddingHorizontal: espaco.l,
    paddingVertical: espaco.m,
    backgroundColor: cores.surface,
    ...sombra,
    shadowOffset: { width: 0, height: -4 },
  },
  erroRodape: { paddingHorizontal: espaco.l, paddingBottom: espaco.s, backgroundColor: cores.paper },
  rodapeRotulo: { fontSize: 12, fontWeight: '600', color: cores.inkMudo },
  rodapeValor: { fontSize: 15, fontWeight: '800', color: cores.ink },
  fundoModal: { flex: 1, backgroundColor: 'rgba(15,23,32,0.45)' },
  folha: { maxHeight: '80%', backgroundColor: cores.paper, borderTopLeftRadius: 24, borderTopRightRadius: 24 },
  alca: { alignSelf: 'center', width: 40, height: 5, borderRadius: 3, backgroundColor: cores.linha, marginTop: 10 },
  folhaCabecalho: { flexDirection: 'row', alignItems: 'center', padding: espaco.l, paddingBottom: espaco.m },
  folhaTitulo: { fontSize: 18, fontWeight: '800', color: cores.ink },
  opcaoItem: { flexDirection: 'row', alignItems: 'center', gap: 12, padding: 12, borderRadius: raio.m, borderWidth: 1.5, borderColor: cores.linha, backgroundColor: cores.surface },
  sucesso: { alignItems: 'center', paddingHorizontal: espaco.xl, paddingBottom: 32 },
  sucessoIcone: { width: 88, height: 88, borderRadius: 44, backgroundColor: cores.verde, alignItems: 'center', justifyContent: 'center', ...sombra, shadowColor: cores.verde, shadowOpacity: 0.35 },
  sucessoTitulo: { fontSize: 24, fontWeight: '800', color: cores.ink, marginTop: 20 },
  sucessoTexto: { fontSize: 14.5, color: cores.inkSuave, textAlign: 'center', marginTop: 6, lineHeight: 21 },
  divisor: { height: 1, backgroundColor: cores.linha },
  linhaResumo: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', gap: 12 },
  resumoRotulo: { fontSize: 13.5, color: cores.inkSuave },
  resumoValor: { flexShrink: 1, fontSize: 14, fontWeight: '700', color: cores.ink, textAlign: 'right' },
  resumoDestaque: { fontSize: 16, fontWeight: '800', color: cores.azul, letterSpacing: 0.3 },
});
