import { Stack } from 'expo-router';
import { cores } from '@/theme/tema';

export default function LayoutLogado() {
  return (
    <Stack
      screenOptions={{
        contentStyle: { backgroundColor: cores.paper },
        headerTintColor: cores.ink,
        headerShadowVisible: false,
        headerStyle: { backgroundColor: cores.paper },
        headerTitleStyle: { fontWeight: '700' },
        headerBackButtonDisplayMode: 'minimal',
      }}
    >
      <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
      <Stack.Screen name="solicitacao/[id]" options={{ title: 'Detalhes da solicitação' }} />
    </Stack>
  );
}
