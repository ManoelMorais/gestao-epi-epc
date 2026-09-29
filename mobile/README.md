# Amperion EPI — app mobile do colaborador (Expo / React Native)

App individual: cada colaborador entra com **DRT e senha** e vê só os próprios dados.

- **Início** — EPIs que estão com ele (com alerta de vencido / vence em breve), situação
  das solicitações, aviso de troca aprovada para retirar e atalho para pedir troca.
- **Solicitações** — só as dele, com busca e filtro por situação
  (em andamento, aprovadas, entregues, recusadas) e detalhe com linha do tempo.
- **Solicitar** — pedido de troca de EPI/EPC no modelo do formulário do SIGME:
  identificação automática do colaborador, item (só os liberados para o cargo), tamanho,
  motivo, quantidade, CA, dados do material antigo (entrega, fabricação, lote, marca,
  relato), até 2 fotos e assinatura na tela.
- **Meu cadastro** — dados funcionais e de contato, EPI/EPC liberados para o cargo, sair.

## Rodando

```bash
npm install
npx expo start        # abra no Expo Go (Android/iOS) lendo o QR code
npx expo start --web  # ou no navegador
```

Login de demonstração: o DRT de qualquer colaborador do seed com a senha `123456`.
O `10234` (Roberto, eletricista) tem um exemplo de cada situação; o `12101` (inativo) é bloqueado.

## Estrutura

- `src/app/` — telas (Expo Router): `login`, `(app)/(tabs)/index | solicitacoes | nova | perfil`
  e `(app)/solicitacao/[id]`.
- `src/services/api.ts` — contrato `GestaoEpiApi`. Todo método recebe a sessão e só trabalha
  com o colaborador dela. Hoje atendido por `apiEmMemoria.ts` + `src/data/seed.ts`;
  quando a API ASP.NET Core existir, basta uma implementação HTTP do mesmo contrato.
- `src/types/dominio.ts` — espelho das entidades de `GestaoEpiEpc.Domain` + `Solicitacao`.
- `src/components/Assinatura.tsx` — painel de assinatura (react-native-svg, sem libs extras).

## Verificação

```bash
npm run typecheck
npx expo lint
```
