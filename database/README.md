# Banco de dados — Supabase (PostgreSQL)

O app desktop grava no PostgreSQL do Supabase via Entity Framework Core
(`src/GestaoEpiEpc.Infrastructure/Persistence`). Sem connection string ele roda em memória.

## Conectar a um projeto Supabase

1. No painel do projeto: **Connect** → copie a string do **Session pooler** (porta 5432).
   Não use a "Direct connection" (`db.<ref>.supabase.co`): ela só funciona via IPv6.
2. Preencha `src/GestaoEpiEpc.Desktop/appsettings.Local.json` (fora do git) no formato:

   ```
   Host=aws-0-<regiao>.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.<ref>;Password=<senha>;SSL Mode=Require;Trust Server Certificate=true
   ```

3. Rode o desktop. Na abertura ele aplica as migrations e, se o banco estiver vazio,
   grava os dados de exemplo da Amperion Energia.

## O que é criado

- 14 tabelas em snake_case (incluindo `sessao_app`, das sessões do app): `unidade`, `cargo`, `colaborador`, `categoria_item`, `item_epi_epc`,
  `cargo_item_permitido`, `motivo_movimentacao`, `usuario`, `entrega`, `entrega_item`, `log_auditoria`,
  `solicitacao` e `evento_solicitacao` (pedidos de troca feitos no app mobile e a linha do tempo de cada um).
- Os dados de exemplo são um port fiel de `mobile/src/data/seed.ts` (mesmo gerador pseudoaleatório e
  mesmas sementes): desktop e app mostram as mesmas entregas, solicitações e protocolos.
- `colaborador.senha_hash` guarda a senha do app em bcrypt (nunca em texto), verificável tanto pelo C# quanto pelo Postgres. Senha de demonstração: `123456`.
- Enums gravados como texto (`Troca`, `Confirmada`...), legíveis no Table Editor.
- **RLS ligado em todas as tabelas, sem policies**: a API REST pública do Supabase (chave anon)
  não enxerga nada; o app conecta como `postgres`, dono das tabelas, e não é afetado.

## API do app mobile (funções RPC)

O app mobile fala **direto com o Supabase**, sem servidor intermediário — assim o APK funciona em
qualquer rede. Cada função de `public` vira `POST https://<ref>.supabase.co/rest/v1/rpc/<nome>`,
chamada com a chave *publishable* (`mobile/src/services/apiSupabase.ts`):

| Função | O que faz |
|---|---|
| `app_login(p_drt, p_senha)` | Confere a senha (bcrypt, via `pgcrypto`) e devolve `{ token, colaborador }` |
| `app_resumo(p_token)` | Itens em posse, contagem por status e 3 solicitações recentes |
| `app_itens_em_posse(p_token)` | Última movimentação confirmada de cada item + validade calculada |
| `app_listar_solicitacoes(p_token, p_grupo, p_termo)` | Solicitações do colaborador (grupo `andamento` = pendente + em análise) |
| `app_obter_solicitacao(p_token, p_id)` | Uma solicitação — `null` se for de outra pessoa |
| `app_itens_elegiveis(p_token)` | Itens liberados para o cargo (regra de elegibilidade) |
| `app_motivos()` | Motivos de troca/reposição |
| `app_criar_solicitacao(p_token, p_input)` | Cria o pedido com as mesmas validações do `SolicitacaoService` |
| `app_logout(p_token)` | Encerra a sessão |

- O SQL fica em `src/GestaoEpiEpc.Infrastructure/Persistence/Sql/ApiMobile.sql` (aplicado pela migration `ApiMobile`).
- As tabelas seguem com RLS e **sem policies**: a chave do app não lê tabela nenhuma, só executa essas funções
  (`SECURITY DEFINER`), e cada uma só devolve dados do colaborador dono do token.
- Auxiliares ficam no schema `app_privado`, que o Supabase não expõe. O token de sessão é guardado como SHA-256 (`sessao_app`).
- Categorias e motivos são identificados no app pelo código estável (`cat-pes`, `mot-validade`).
- `ApiMobileTests` compara o resultado das funções com os serviços em C# sobre o mesmo banco
  (inclusive o caminho: pedido criado no app → aprovado no desktop → status visto no app).

`schema.sql` é o mesmo schema em SQL puro (idempotente), caso prefira criar pelo SQL Editor do Supabase.

## Migrations

`dotnet ef` é ferramenta local do repositório (`dotnet tool restore` na primeira vez):

```
dotnet ef migrations add <Nome> --project src/GestaoEpiEpc.Infrastructure --output-dir Persistence/Migrations
```

## Testes de integração

`tests/GestaoEpiEpc.Tests/PostgresIntegracaoTests.cs` roda os serviços contra um Postgres real
quando `GESTAOEPI_TESTE_POSTGRES` está definida (cada teste cria e apaga um banco próprio):

```
docker run -d --name gestaoepi-pg -e POSTGRES_PASSWORD=postgres -p 55432:5432 postgres:17
$env:GESTAOEPI_TESTE_POSTGRES = "Host=localhost;Port=55432;Username=postgres;Password=postgres"
dotnet test
```
