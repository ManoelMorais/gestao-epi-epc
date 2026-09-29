import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { Tabs } from 'expo-router';
import { StyleSheet, View, type ColorValue } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { cores, sombra, type NomeIcone } from '@/theme/tema';

function IconeAba({ nome, cor, focado }: { nome: NomeIcone; cor: ColorValue; focado: boolean }) {
  return (
    <View style={[estilos.iconeAba, focado && { backgroundColor: cores.azulSuave }]}>
      <MaterialCommunityIcons name={nome} size={22} color={cor} />
    </View>
  );
}

export default function LayoutAbas() {
  const insets = useSafeAreaInsets();
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: cores.azul,
        tabBarInactiveTintColor: cores.inkMudo,
        tabBarLabelStyle: { fontSize: 11.5, fontWeight: '700' },
        tabBarStyle: [estilos.barra, { height: 72 + insets.bottom, paddingBottom: insets.bottom + 10 }],
        sceneStyle: { backgroundColor: cores.paper },
      }}
    >
      <Tabs.Screen
        name="index"
        options={{
          title: 'Início',
          tabBarIcon: ({ color, focused }) => <IconeAba nome={focused ? 'view-dashboard' : 'view-dashboard-outline'} cor={color} focado={focused} />,
        }}
      />
      <Tabs.Screen
        name="solicitacoes"
        options={{
          title: 'Solicitações',
          tabBarIcon: ({ color, focused }) => <IconeAba nome={focused ? 'clipboard-list' : 'clipboard-list-outline'} cor={color} focado={focused} />,
        }}
      />
      <Tabs.Screen
        name="nova"
        options={{
          title: 'Solicitar',
          tabBarIcon: ({ color, focused }) => <IconeAba nome={focused ? 'swap-horizontal-circle' : 'swap-horizontal-circle-outline'} cor={color} focado={focused} />,
        }}
      />
      <Tabs.Screen
        name="perfil"
        options={{
          title: 'Meu cadastro',
          tabBarIcon: ({ color, focused }) => <IconeAba nome={focused ? 'account-circle' : 'account-circle-outline'} cor={color} focado={focused} />,
        }}
      />
    </Tabs>
  );
}

const estilos = StyleSheet.create({
  barra: {
    backgroundColor: cores.surface,
    borderTopWidth: 0,
    paddingTop: 8,
    ...sombra,
    shadowOffset: { width: 0, height: -4 },
  },
  iconeAba: { width: 48, height: 28, borderRadius: 14, alignItems: 'center', justifyContent: 'center' },
});
