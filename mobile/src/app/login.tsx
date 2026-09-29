import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { LinearGradient } from 'expo-linear-gradient';
import { useRef, useState } from 'react';
import { KeyboardAvoidingView, Platform, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { Aviso, Botao, CampoTexto } from '@/components/ui';
import { useSessao } from '@/context/SessaoContext';
import { ErroNegocio } from '@/services/api';
import { cores, espaco, gradienteMarca, raio, sombra } from '@/theme/tema';

export default function TelaLogin() {
  const { entrar } = useSessao();
  const insets = useSafeAreaInsets();
  const campoSenha = useRef<TextInput>(null);

  const [drt, setDrt] = useState('');
  const [senha, setSenha] = useState('');
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  const podeEntrar = drt.trim().length > 0 && senha.length > 0;

  async function aoEntrar() {
    if (!podeEntrar || carregando) return;
    setErro(null);
    setCarregando(true);
    try {
      await entrar(drt, senha);
      // O layout raiz troca para a área logada ao perceber a sessão.
    } catch (e) {
      setErro(e instanceof ErroNegocio ? e.message : 'Não foi possível entrar. Tente novamente.');
      setCarregando(false);
    }
  }

  return (
    <KeyboardAvoidingView style={{ flex: 1, backgroundColor: cores.paper }} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      <ScrollView contentContainerStyle={{ flexGrow: 1 }} keyboardShouldPersistTaps="handled" bounces={false}>
        <LinearGradient colors={gradienteMarca} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }} style={[estilos.topo, { paddingTop: insets.top + 48 }]}>
          {/* Círculos decorativos no fundo do cabeçalho */}
          <View style={[estilos.circulo, { width: 260, height: 260, top: -80, right: -90 }]} />
          <View style={[estilos.circulo, { width: 160, height: 160, bottom: -40, left: -50 }]} />

          <View style={estilos.logo}>
            <MaterialCommunityIcons name="shield-check" size={36} color={cores.azul} />
          </View>
          <Text style={estilos.marca}>Amperion EPI</Text>
          <Text style={estilos.subtitulo}>Gestão de EPI/EPC · Portal do colaborador</Text>
        </LinearGradient>

        <View style={[estilos.cartao, { marginBottom: insets.bottom + 24 }]}>
          <Text style={estilos.titulo}>Entrar</Text>
          <Text style={estilos.descricao}>Use seu DRT e sua senha corporativa.</Text>

          <View style={{ gap: espaco.l, marginTop: espaco.xl }}>
            <CampoTexto
              rotulo="DRT"
              icone="card-account-details-outline"
              placeholder="Seu número de DRT"
              keyboardType="number-pad"
              value={drt}
              onChangeText={(t) => {
                setDrt(t.replace(/\D/g, ''));
                setErro(null);
              }}
              returnKeyType="next"
              onSubmitEditing={() => campoSenha.current?.focus()}
              maxLength={10}
              erro={!!erro}
            />
            <CampoTexto
              ref={campoSenha}
              rotulo="Senha"
              icone="lock-outline"
              placeholder="Sua senha"
              secureTextEntry={!mostrarSenha}
              value={senha}
              onChangeText={(t) => {
                setSenha(t);
                setErro(null);
              }}
              returnKeyType="go"
              onSubmitEditing={aoEntrar}
              erro={!!erro}
              direita={
                <Pressable onPress={() => setMostrarSenha((v) => !v)} hitSlop={10} accessibilityLabel={mostrarSenha ? 'Ocultar senha' : 'Mostrar senha'}>
                  <MaterialCommunityIcons name={mostrarSenha ? 'eye-off-outline' : 'eye-outline'} size={20} color={cores.inkMudo} />
                </Pressable>
              }
            />

            {erro && <Aviso tipo="erro" texto={erro} />}

            <Botao titulo="Entrar" icone="login" onPress={aoEntrar} carregando={carregando} desabilitado={!podeEntrar} style={{ marginTop: espaco.s }} />
          </View>

          <View style={estilos.demo}>
            <MaterialCommunityIcons name="information-outline" size={16} color={cores.inkSuave} />
            <Text style={estilos.demoTexto}>
              Ambiente de demonstração — use o DRT de qualquer colaborador (ex.: <Text style={estilos.negrito}>10234</Text>) e a
              senha <Text style={estilos.negrito}>123456</Text>.
            </Text>
          </View>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const estilos = StyleSheet.create({
  topo: {
    alignItems: 'center',
    paddingBottom: 72,
    overflow: 'hidden',
  },
  circulo: { position: 'absolute', borderRadius: 999, backgroundColor: 'rgba(255,255,255,0.08)' },
  logo: {
    width: 72,
    height: 72,
    borderRadius: 22,
    backgroundColor: '#FFFFFF',
    alignItems: 'center',
    justifyContent: 'center',
    ...sombra,
    shadowOpacity: 0.2,
  },
  marca: { color: '#FFFFFF', fontSize: 26, fontWeight: '800', marginTop: 16, letterSpacing: 0.2 },
  subtitulo: { color: 'rgba(255,255,255,0.8)', fontSize: 14, marginTop: 4 },
  cartao: {
    marginTop: -44,
    marginHorizontal: espaco.l,
    backgroundColor: cores.surface,
    borderRadius: raio.g + 4,
    padding: espaco.xl + 4,
    ...sombra,
  },
  titulo: { fontSize: 22, fontWeight: '800', color: cores.ink },
  descricao: { fontSize: 14, color: cores.inkSuave, marginTop: 4 },
  demo: {
    flexDirection: 'row',
    gap: 8,
    marginTop: espaco.xl,
    padding: 12,
    backgroundColor: cores.paper,
    borderRadius: raio.m,
  },
  demoTexto: { flex: 1, fontSize: 12.5, color: cores.inkSuave, lineHeight: 18 },
  negrito: { fontWeight: '700', color: cores.ink },
});
