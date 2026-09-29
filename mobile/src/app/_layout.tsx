import { Stack } from 'expo-router';
import { StatusBar } from 'expo-status-bar';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { SessaoProvider, useSessao } from '@/context/SessaoContext';
import { cores } from '@/theme/tema';

export default function LayoutRaiz() {
  return (
    <SafeAreaProvider>
      <SessaoProvider>
        <StatusBar style="auto" />
        <Navegacao />
      </SessaoProvider>
    </SafeAreaProvider>
  );
}

function Navegacao() {
  const { sessao } = useSessao();

  // Sem sessão, só o login existe; com sessão, o login deixa de existir.
  // O Expo Router redireciona sozinho quando a sessão muda (entrar/sair).
  return (
    <Stack screenOptions={{ headerShown: false, contentStyle: { backgroundColor: cores.paper } }}>
      <Stack.Protected guard={!!sessao}>
        <Stack.Screen name="(app)" />
      </Stack.Protected>
      <Stack.Protected guard={!sessao}>
        <Stack.Screen name="login" />
      </Stack.Protected>
    </Stack>
  );
}
