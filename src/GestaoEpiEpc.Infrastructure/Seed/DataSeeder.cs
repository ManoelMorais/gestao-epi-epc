using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using GestaoEpiEpc.Application.Seguranca;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;
using GestaoEpiEpc.Infrastructure.InMemory;

namespace GestaoEpiEpc.Infrastructure.Seed;

/// <summary>
/// Dados de exemplo do sistema. É um port fiel de <c>mobile/src/data/seed.ts</c> — o app mobile é a
/// referência dos dados fixos (unidades, cargos, catálogo, elegibilidade, motivos, colaboradores) e do
/// histórico de demonstração. Os geradores usam o mesmo algoritmo pseudoaleatório (mulberry32) com as
/// mesmas sementes, então desktop e mobile mostram exatamente as mesmas entregas e solicitações.
/// Os IDs são fixos, derivados dos IDs do mobile (ex.: "col-roberto"), para o seed ser estável.
/// </summary>
internal static class DataSeeder
{
    /// <summary>Senha de demonstração do app, igual para todos (a mesma de <c>SENHA_DEMO</c> no mobile).</summary>
    public const string SenhaDemo = "123456";

    // PBKDF2 é propositalmente lento; calcula uma vez só por processo.
    private static readonly Lazy<string> HashSenhaDemo = new(() => HashSenha.Gerar(SenhaDemo));

    public static void Popular(InMemoryDataStore store)
    {
        var agora = DateTime.Now;
        DateTime DiasAtras(int dias, int horas = 0, int minutos = 0)
        {
            var d = agora.AddDays(-dias);
            return new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0).AddHours(-horas).AddMinutes(-minutos);
        }

        // ---- Unidades ------------------------------------------------------
        var aps = new Unidade { Id = IdFixo("uni-aps"), Nome = "Amperion Sul", Sigla = "APS", Cidade = "Porto Sereno" };
        var apn = new Unidade { Id = IdFixo("uni-apn"), Nome = "Amperion Norte", Sigla = "APN", Cidade = "Vale do Norte" };
        store.Unidades.AddRange([aps, apn]);
        var unidades = new Dictionary<string, Unidade> { ["uni-aps"] = aps, ["uni-apn"] = apn };

        // ---- Cargos --------------------------------------------------------
        var cargos = new Dictionary<string, Cargo>();
        foreach (var (id, nome) in new[]
        {
            ("car-eletricista", "Eletricista de Rede"),
            ("car-auxiliar", "Auxiliar de Almoxarifado"),
            ("car-sst", "Técnico de Segurança do Trabalho"),
            ("car-adm", "Analista Administrativo"),
            ("car-supervisor", "Supervisor de Campo"),
            ("car-manutencao", "Técnico de Manutenção"),
        })
            cargos[id] = new Cargo { Id = IdFixo(id), Nome = nome };
        store.Cargos.AddRange(cargos.Values);

        // ---- Categorias e catálogo -------------------------------------------
        var categorias = new Dictionary<string, CategoriaItem>();
        foreach (var (id, nome, tipo) in new[]
        {
            ("cat-cabeca", "Proteção da Cabeça", TipoItem.Epi),
            ("cat-maos", "Proteção das Mãos", TipoItem.Epi),
            ("cat-quedas", "Proteção contra Quedas", TipoItem.Epi),
            ("cat-vestimenta", "Vestimenta", TipoItem.Epi),
            ("cat-auditiva", "Proteção Auditiva", TipoItem.Epi),
            ("cat-pes", "Proteção dos Pés", TipoItem.Epi),
            ("cat-sinalizacao", "Sinalização e Isolamento", TipoItem.Epc),
        })
            categorias[id] = new CategoriaItem { Id = IdFixo(id), Codigo = id, Nome = nome, Tipo = tipo };
        store.CategoriasItem.AddRange(categorias.Values);

        var itens = new Dictionary<string, ItemEpiEpc>();
        var categoriaDoItem = new Dictionary<string, string>();
        foreach (var (id, codigo, nome, categoria, ca, validade, tamanho) in new (string, string, string, string, string?, int?, bool)[]
        {
            ("itm-capacete", "EPI-001", "Capacete de Segurança Classe B", "cat-cabeca", "31469", 60, false),
            ("itm-oculos", "EPI-002", "Óculos de Proteção Incolor", "cat-cabeca", "25763", 24, false),
            ("itm-luva-isolante", "EPI-003", "Luva Isolante de Borracha Classe 2", "cat-maos", "28871", 12, true),
            ("itm-cinto", "EPI-004", "Cinto de Segurança Tipo Paraquedista", "cat-quedas", "34115", 24, true),
            ("itm-nomex", "EPI-005", "Uniforme NOMEX Manga Longa", "cat-vestimenta", "30044", 24, true),
            ("itm-luva-raspa", "EPI-006", "Luva de Raspa Reforçada", "cat-maos", "19207", 6, true),
            ("itm-talabarte", "EPI-007", "Talabarte Duplo em Y", "cat-quedas", "37788", 24, false),
            ("itm-auricular", "EPI-008", "Protetor Auricular Tipo Plug", "cat-auditiva", "5745", 6, false),
            ("itm-bota", "EPI-009", "Bota de Segurança com Bico de Aço", "cat-pes", "40311", 12, true),
            ("itm-fita", "EPC-001", "Fita Zebrada de Sinalização", "cat-sinalizacao", null, null, false),
            ("itm-cone", "EPC-002", "Cone de Sinalização", "cat-sinalizacao", null, null, false),
            ("itm-sinalizador", "EPC-003", "Sinalizador Luminoso Portátil", "cat-sinalizacao", null, null, false),
        })
        {
            itens[id] = new ItemEpiEpc
            {
                Id = IdFixo(id), Codigo = codigo, Nome = nome, CategoriaId = categorias[categoria].Id,
                NumeroCa = ca, ValidadePadraoMeses = validade, PossuiTamanho = tamanho
            };
            categoriaDoItem[id] = categoria;
        }
        store.Itens.AddRange(itens.Values);

        // ---- Elegibilidade por cargo (CargoItemPermitido) ----------------------
        var itensPermitidosPorCargo = new Dictionary<string, string[]>
        {
            ["car-eletricista"] = ["itm-capacete", "itm-oculos", "itm-luva-isolante", "itm-cinto", "itm-nomex", "itm-talabarte", "itm-bota", "itm-fita", "itm-cone"],
            ["car-auxiliar"] = ["itm-capacete", "itm-oculos", "itm-luva-raspa", "itm-bota"],
            ["car-sst"] = ["itm-capacete", "itm-oculos", "itm-auricular", "itm-fita", "itm-cone", "itm-sinalizador"],
            // Função de escritório: nenhum EPI/EPC de campo é elegível.
            ["car-adm"] = [],
            ["car-supervisor"] = ["itm-capacete", "itm-oculos", "itm-luva-isolante", "itm-nomex", "itm-bota", "itm-cone"],
            ["car-manutencao"] = ["itm-capacete", "itm-oculos", "itm-luva-raspa", "itm-auricular", "itm-cinto", "itm-talabarte", "itm-bota"],
        };
        foreach (var (cargo, permitidos) in itensPermitidosPorCargo)
            foreach (var item in permitidos)
                store.CargoItemPermitidos.Add(new CargoItemPermitido { Id = IdFixo($"{cargo}:{item}"), CargoId = cargos[cargo].Id, ItemId = itens[item].Id });

        // ---- Motivos ---------------------------------------------------------
        var motivos = new Dictionary<string, MotivoMovimentacao>();
        foreach (var (id, descricao, tipo, semDevolucao) in new[]
        {
            ("mot-novo", "Novo colaborador", TipoMovimentacao.EntregaInicial, false),
            ("mot-desgaste", "Desgaste natural", TipoMovimentacao.Troca, false),
            ("mot-avaria", "Dano em serviço", TipoMovimentacao.Troca, false),
            ("mot-validade", "Validade vencida", TipoMovimentacao.Troca, false),
            ("mot-tamanho", "Tamanho inadequado", TipoMovimentacao.Troca, false),
            ("mot-defeito", "Defeito de fabricação", TipoMovimentacao.Troca, false),
            ("mot-perda", "Perda ou extravio", TipoMovimentacao.Reposicao, true),
            ("mot-desligamento", "Devolução por desligamento", TipoMovimentacao.Devolucao, false),
        })
            motivos[id] = new MotivoMovimentacao { Id = IdFixo(id), Codigo = id, Descricao = descricao, TipoAplicavel = tipo, SemDevolucao = semDevolucao };
        store.MotivosMovimentacao.AddRange(motivos.Values);

        // ---- Usuários do desktop (gestão/SST/RH/admin) e facilitadores do almoxarifado ----
        var admin = new Usuario { Id = IdFixo("usr-admin"), Nome = "Manoel Almeida de Morais", Email = "manoel.morais@amperion.com.br", Perfil = PerfilUsuario.Administrador, UnidadeId = aps.Id };
        var gestao = new Usuario { Id = IdFixo("usr-gestao"), Nome = "Ana Beatriz Souza", Email = "ana.souza@amperion.com.br", Perfil = PerfilUsuario.Gestao, UnidadeId = aps.Id };
        var sst = new Usuario { Id = IdFixo("usr-sst"), Nome = "Carlos Eduardo Lima", Email = "carlos.lima@amperion.com.br", Perfil = PerfilUsuario.SegurancaTrabalho, UnidadeId = aps.Id };
        var rh = new Usuario { Id = IdFixo("usr-rh"), Nome = "Fernanda Rocha", Email = "fernanda.rocha@amperion.com.br", Perfil = PerfilUsuario.Rh, UnidadeId = aps.Id };
        var facilitadorSul = new Usuario { Id = IdFixo("usr-almox-aps"), Nome = "João Pedro Santos", Email = "joao.santos@amperion.com.br", Perfil = PerfilUsuario.Facilitador, UnidadeId = aps.Id };
        var facilitadorNorte = new Usuario { Id = IdFixo("usr-almox-apn"), Nome = "Bruna Martins Souza", Email = "bruna.souza@amperion.com.br", Perfil = PerfilUsuario.Facilitador, UnidadeId = apn.Id };
        store.Usuarios.AddRange([admin, gestao, sst, rh, facilitadorSul, facilitadorNorte]);

        // ---- Colaboradores (cada um tem o próprio acesso ao app) ---------------------
        var colaboradores = new List<(string Chave, Colaborador Colaborador)>();
        void C(string id, string drt, string nome, string cargo, string area, string unidade, string admissao, string telefone, StatusColaborador status = StatusColaborador.Ativo) =>
            colaboradores.Add((id, new Colaborador
            {
                Id = IdFixo(id), Drt = drt, Nome = nome, CargoId = cargos[cargo].Id, Area = area, UnidadeId = unidades[unidade].Id,
                Admissao = DateOnly.ParseExact(admissao, "yyyy-MM-dd", CultureInfo.InvariantCulture), Telefone = telefone, Status = status,
                Email = EmailCorporativo(nome), SenhaHash = HashSenhaDemo.Value
            }));

        C("col-roberto", "10234", "Roberto Carlos Nascimento", "car-eletricista", "Manutenção de Rede", "uni-aps", "2019-03-11", "(79) 99812-4410");
        C("col-juliana", "10567", "Juliana Alves Pereira", "car-eletricista", "Manutenção de Rede", "uni-aps", "2020-07-01", "(79) 99734-2281");
        C("col-marcos", "10890", "Marcos Vinícius Teixeira", "car-auxiliar", "Almoxarifado", "uni-aps", "2021-02-15", "(79) 99655-0192");
        C("col-patricia", "11023", "Patrícia Gomes Ferreira", "car-sst", "Segurança do Trabalho", "uni-aps", "2018-09-03", "(79) 99901-7763");
        C("col-rafael", "11345", "Rafael Costa Andrade", "car-adm", "Administrativo", "uni-aps", "2022-01-10", "(79) 99588-3304");
        C("col-camila", "11789", "Camila Souza Ribeiro", "car-eletricista", "Manutenção de Rede", "uni-apn", "2021-05-24", "(65) 99672-1185");
        C("col-fernando", "12012", "Fernando Henrique Lopes", "car-supervisor", "Manutenção de Rede", "uni-aps", "2016-11-07", "(79) 99843-5520");
        C("col-bianca", "12045", "Bianca Oliveira Cardoso", "car-manutencao", "Manutenção de Rede", "uni-aps", "2023-04-17", "(79) 99710-6648");
        C("col-diego", "12078", "Diego Martins Rocha", "car-eletricista", "Manutenção de Rede", "uni-aps", "2019-08-19", "(79) 99627-9031", StatusColaborador.Afastado);
        C("col-larissa", "12101", "Larissa Fernandes Melo", "car-auxiliar", "Almoxarifado", "uni-aps", "2020-10-05", "(79) 99555-4417", StatusColaborador.Inativo);
        C("col-eduardo", "12134", "Eduardo Pereira Gomes", "car-manutencao", "Manutenção de Rede", "uni-aps", "2022-06-13", "(79) 99788-2256");
        C("col-vanessa", "12167", "Vanessa Almeida Torres", "car-adm", "Administrativo", "uni-aps", "2023-09-01", "(79) 99604-8872");
        C("col-gustavo", "12190", "Gustavo Henrique Silva", "car-eletricista", "Manutenção de Rede", "uni-apn", "2020-02-03", "(65) 99745-3319");
        C("col-renata", "12223", "Renata Cristina Barbosa", "car-supervisor", "Manutenção de Rede", "uni-apn", "2017-12-11", "(65) 99831-0074");
        C("col-thiago", "12256", "Thiago Souza Martins", "car-sst", "Segurança do Trabalho", "uni-apn", "2021-08-23", "(65) 99690-5528");
        C("col-amanda", "12289", "Amanda Ribeiro Costa", "car-auxiliar", "Almoxarifado", "uni-apn", "2024-01-08", "(65) 99577-1143");
        C("col-bruno", "12312", "Bruno César Andrade", "car-manutencao", "Manutenção de Rede", "uni-apn", "2022-10-31", "(65) 99802-6690");
        C("col-isabela", "12345", "Isabela Nunes Carvalho", "car-eletricista", "Manutenção de Rede", "uni-apn", "2023-03-20", "(65) 99718-4402");
        store.Colaboradores.AddRange(colaboradores.Select(c => c.Colaborador));

        var colaboradorPorChave = colaboradores.ToDictionary(c => c.Chave, c => c.Colaborador);
        var cargoDoColaborador = colaboradores.ToDictionary(c => c.Chave, c => cargos.Single(x => x.Value.Id == c.Colaborador.CargoId).Key);

        string? TamanhoDoColaborador(string colaborador, string item)
        {
            if (!itens[item].PossuiTamanho) return null;
            var t = Tamanhos.TryGetValue(colaborador, out var cadastro) ? cadastro : (Roupa: "M", Luva: "9", Bota: "40");
            return categoriaDoItem[item] switch
            {
                "cat-pes" => t.Bota,
                "cat-maos" => t.Luva,
                _ => t.Roupa
            };
        }

        // ---- Entregas (histórico já efetivado pelo almoxarifado) -----------------------
        var sequencialEntrega = 0;
        void Entregar(string colaborador, string motivo, TipoMovimentacao tipo, DateTime data, params (string Item, int Quantidade)[] lista)
        {
            sequencialEntrega++;
            // O mobile também gera kits vazios quando o sorteio não escolhe nenhum item; no banco eles não fazem sentido.
            if (lista.Length == 0) return;

            var col = colaboradorPorChave[colaborador];
            var entrega = new Entrega
            {
                Id = IdFixo($"ent-{sequencialEntrega}"),
                ColaboradorId = col.Id,
                FacilitadorId = col.UnidadeId == aps.Id ? facilitadorSul.Id : facilitadorNorte.Id,
                UnidadeId = col.UnidadeId,
                MotivoId = motivos[motivo].Id,
                TipoMovimentacao = tipo,
                Status = StatusEntrega.Confirmada,
                DataHora = data
            };
            entrega.Itens = lista.Select((i, indice) => new EntregaItem
            {
                Id = IdFixo($"ent-{sequencialEntrega}:{indice}"),
                EntregaId = entrega.Id,
                ItemId = itens[i.Item].Id,
                Quantidade = i.Quantidade,
                Tamanho = TamanhoDoColaborador(colaborador, i.Item)
            }).ToList();
            store.Entregas.Add(entrega);
        }

        // Roberto (usuário principal da demonstração): kit antigo, com um item vencido e um
        // vencendo, e uma troca recente de luva por defeito.
        Entregar("col-roberto", "mot-novo", TipoMovimentacao.EntregaInicial, DiasAtras(700), ("itm-capacete", 1), ("itm-cinto", 1), ("itm-talabarte", 1));
        Entregar("col-roberto", "mot-validade", TipoMovimentacao.Troca, DiasAtras(385), ("itm-bota", 1));
        Entregar("col-roberto", "mot-desgaste", TipoMovimentacao.Troca, DiasAtras(340), ("itm-nomex", 2), ("itm-oculos", 1));
        Entregar("col-roberto", "mot-desgaste", TipoMovimentacao.Troca, DiasAtras(352), ("itm-luva-isolante", 2));
        Entregar("col-roberto", "mot-defeito", TipoMovimentacao.Troca, DiasAtras(16, 3), ("itm-luva-isolante", 1));
        Entregar("col-roberto", "mot-novo", TipoMovimentacao.EntregaInicial, DiasAtras(200), ("itm-cone", 4), ("itm-fita", 1));

        // Demais colaboradores: kit inicial + trocas aleatórias, respeitando a elegibilidade.
        var aleatorio = new Mulberry32(42);
        foreach (var (chave, _) in colaboradores)
        {
            if (chave == "col-roberto") continue;
            var permitidos = itensPermitidosPorCargo[cargoDoColaborador[chave]];
            if (permitidos.Length == 0) continue;

            var inicio = aleatorio.Inteiro(200, 600);
            var kit = permitidos.Where(_ => aleatorio.Numero() < 0.7).ToList();
            Entregar(chave, "mot-novo", TipoMovimentacao.EntregaInicial, DiasAtras(inicio), kit.Select(id => (id, 1)).ToArray());

            var trocas = aleatorio.Inteiro(1, 4);
            for (var i = 0; i < trocas; i++)
            {
                var item = aleatorio.Escolher(permitidos);
                var motivo = aleatorio.Escolher(["mot-desgaste", "mot-avaria", "mot-validade"]);
                Entregar(chave, motivo, TipoMovimentacao.Troca, DiasAtras(aleatorio.Inteiro(5, inicio)), (item, 1));
            }
        }

        // ---- Solicitações (pedidos feitos pelos colaboradores no app) --------------------
        var solicitacoes = new List<Solicitacao>();
        void Solicitar(string colaborador, string item, string motivo, StatusSolicitacao status, DateTime criadaEm, string relato,
            string? lote = null, string? marca = null, string? fabricacao = null, int quantidade = 1, string? recusa = null, int entregueHaDias = 300)
        {
            var col = colaboradorPorChave[colaborador];
            var semDevolucao = motivos[motivo].SemDevolucao;
            var solicitacao = new Solicitacao
            {
                Id = IdFixo($"sol-{solicitacoes.Count + 1}"),
                Protocolo = string.Empty,
                ColaboradorId = col.Id,
                ItemId = itens[item].Id,
                MotivoId = motivos[motivo].Id,
                Tamanho = TamanhoDoColaborador(colaborador, item),
                Quantidade = quantidade,
                Relato = relato,
                MaterialLote = semDevolucao ? null : lote,
                MaterialMarca = semDevolucao ? null : marca,
                MaterialFabricacao = semDevolucao ? null : fabricacao,
                MaterialDataEntrega = semDevolucao ? null : DateOnly.FromDateTime(DiasAtras(entregueHaDias)),
                Assinatura = AssinaturaDemo,
                Status = status,
                CriadaEm = criadaEm
            };
            var sigla = col.UnidadeId == aps.Id ? aps.Sigla : apn.Sigla;
            solicitacao.Historico = HistoricoAte(solicitacao, col.Nome, sigla, recusa);
            solicitacoes.Add(solicitacao);
        }

        // Roberto: uma solicitação em cada etapa, para a demonstração.
        Solicitar("col-roberto", "itm-bota", "mot-validade", StatusSolicitacao.Pendente, DiasAtras(0, 2), "Bota passou da validade e o solado está descolando na ponta.", lote: "BT-2291", marca: "Marluvas", fabricacao: "03/2024", entregueHaDias: 385);
        Solicitar("col-roberto", "itm-oculos", "mot-avaria", StatusSolicitacao.EmAnalise, DiasAtras(1, 5), "Lente riscou durante poda de árvore próxima à rede.", lote: "OC-7710", marca: "Kalipso", fabricacao: "11/2024", entregueHaDias: 340);
        Solicitar("col-roberto", "itm-nomex", "mot-desgaste", StatusSolicitacao.Aprovada, DiasAtras(4), "Uniforme com costura aberta na manga e tecido desgastado.", lote: "NX-0415", marca: "Protecta", fabricacao: "06/2024", quantidade: 2, entregueHaDias: 340);
        Solicitar("col-roberto", "itm-luva-isolante", "mot-defeito", StatusSolicitacao.Entregue, DiasAtras(18), "Luva apresentou fissura no teste de inflação antes do uso.", lote: "LI-3302", marca: "Orion", fabricacao: "01/2025", entregueHaDias: 352);
        Solicitar("col-roberto", "itm-capacete", "mot-desgaste", StatusSolicitacao.Recusada, DiasAtras(47), "Capacete com a aba riscada.", lote: "CP-1187", marca: "MSA", fabricacao: "02/2023", entregueHaDias: 700,
            recusa: "Riscos superficiais não comprometem a proteção e o item está dentro da validade (vence em 2028).");
        Solicitar("col-roberto", "itm-cone", "mot-perda", StatusSolicitacao.Entregue, DiasAtras(95), "Um cone ficou na via após atendimento emergencial noturno e não foi localizado.");

        // Demais colaboradores: algumas solicitações aleatórias, sempre de itens elegíveis.
        aleatorio = new Mulberry32(7);
        string[] relatos =
        [
            "Item desgastado pelo uso contínuo em campo.",
            "Material danificado durante atividade na rede.",
            "Item com validade vencida conforme etiqueta.",
            "Tamanho não ficou adequado após ajuste de uniforme.",
        ];
        foreach (var (chave, col) in colaboradores)
        {
            if (chave == "col-roberto" || col.Status == StatusColaborador.Inativo) continue;
            var permitidos = itensPermitidosPorCargo[cargoDoColaborador[chave]];
            if (permitidos.Length == 0) continue;

            var quantidade = aleatorio.Inteiro(1, 4);
            for (var i = 0; i < quantidade; i++)
            {
                var dias = aleatorio.Inteiro(0, 120);
                var status = dias < 3 ? aleatorio.Escolher([StatusSolicitacao.Pendente, StatusSolicitacao.EmAnalise])
                    : dias < 10 ? aleatorio.Escolher([StatusSolicitacao.Aprovada, StatusSolicitacao.Entregue])
                    : aleatorio.Numero() < 0.15 ? StatusSolicitacao.Recusada : StatusSolicitacao.Entregue;

                // Mesma ordem de sorteio dos argumentos no seed.ts (item, motivo, horas, relato, lote, mês, ano).
                var item = aleatorio.Escolher(permitidos);
                var motivo = aleatorio.Escolher(["mot-desgaste", "mot-avaria", "mot-validade", "mot-tamanho"]);
                var criadaEm = DiasAtras(dias, aleatorio.Inteiro(0, 8));
                var relato = aleatorio.Escolher(relatos);
                var lote = $"LT-{aleatorio.Inteiro(1000, 9999)}";
                var mes = aleatorio.Inteiro(1, 13);
                var ano = aleatorio.Inteiro(2022, 2026);
                Solicitar(chave, item, motivo, status, criadaEm, relato, lote: lote, fabricacao: $"{mes:D2}/{ano}",
                    recusa: "Item dentro da validade e sem avaria que justifique a troca.");
            }
        }

        // Protocolos em ordem cronológica, como seriam emitidos na vida real.
        var sequencial = SolicitacaoService.SequencialInicialProtocolo;
        foreach (var s in solicitacoes.OrderBy(s => s.CriadaEm))
            s.Protocolo = SolicitacaoService.FormatarProtocolo(s.CriadaEm.Year, ++sequencial);

        store.Solicitacoes.AddRange(solicitacoes);
    }

    /// <summary>Linha do tempo de uma solicitação até o status informado (mesmos responsáveis e prazos do app).</summary>
    private static List<EventoSolicitacao> HistoricoAte(Solicitacao s, string nomeColaborador, string siglaUnidade, string? motivoRecusa)
    {
        var eventos = new List<EventoSolicitacao>();
        void Evento(StatusSolicitacao status, int horas, string responsavel, string? comentario) =>
            eventos.Add(new EventoSolicitacao
            {
                Id = IdFixo($"{s.Id}:{status}"),
                SolicitacaoId = s.Id,
                Status = status,
                DataHora = s.CriadaEm.AddHours(horas),
                Responsavel = responsavel,
                Comentario = comentario
            });

        const string analista = "Carlos Eduardo Lima (SST)";
        var almoxarifado = $"Almoxarifado {siglaUnidade}";

        Evento(StatusSolicitacao.Pendente, 0, nomeColaborador, "Solicitação enviada pelo app.");
        if (s.Status == StatusSolicitacao.Pendente) return eventos;

        Evento(StatusSolicitacao.EmAnalise, 3, analista, null);
        if (s.Status == StatusSolicitacao.EmAnalise) return eventos;

        if (s.Status == StatusSolicitacao.Recusada)
        {
            Evento(StatusSolicitacao.Recusada, 20, analista, motivoRecusa);
            return eventos;
        }

        Evento(StatusSolicitacao.Aprovada, 20, analista, $"Retire o item no {almoxarifado}.");
        if (s.Status == StatusSolicitacao.Aprovada) return eventos;

        Evento(StatusSolicitacao.Entregue, 46, $"{almoxarifado} — João Pedro Santos", "Item entregue e material antigo recolhido.");
        return eventos;
    }

    // Tamanho "de cadastro" de cada colaborador, usado para gerar histórico coerente.
    private static readonly Dictionary<string, (string Roupa, string Luva, string Bota)> Tamanhos = new()
    {
        ["col-roberto"] = ("G", "10", "42"),
        ["col-juliana"] = ("M", "8", "37"),
        ["col-marcos"] = ("G", "9", "41"),
        ["col-patricia"] = ("P", "7", "36"),
        ["col-camila"] = ("M", "8", "38"),
        ["col-fernando"] = ("GG", "11", "43"),
        ["col-bianca"] = ("P", "8", "37"),
        ["col-diego"] = ("G", "10", "42"),
        ["col-larissa"] = ("M", "8", "38"),
        ["col-eduardo"] = ("G", "9", "41"),
        ["col-gustavo"] = ("M", "9", "42"),
        ["col-renata"] = ("P", "7", "36"),
        ["col-thiago"] = ("M", "9", "40"),
        ["col-amanda"] = ("P", "7", "38"),
        ["col-bruno"] = ("G", "10", "43"),
        ["col-isabela"] = ("M", "8", "37"),
    };

    /// <summary>Assinatura de exemplo (caminhos SVG em uma área de 300×120), a mesma do app.</summary>
    private const string AssinaturaDemo =
        "M20 80 C 35 30, 55 30, 60 70 S 80 110, 95 60 M95 60 C 105 35, 120 40, 118 75 M130 72 C 140 40, 160 40, 165 70 C 170 95, 185 95, 195 60 M200 65 L 280 58";

    /// <summary>"Roberto Carlos Nascimento" → "roberto.nascimento@amperion.com.br" (sem acentos), como no app.</summary>
    private static string EmailCorporativo(string nome)
    {
        var semAcento = new string(nome.ToLowerInvariant().Normalize(NormalizationForm.FormD)
            .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark).ToArray());
        var partes = semAcento.Split(' ');
        return $"{partes[0]}.{partes[^1]}@amperion.com.br";
    }

    /// <summary>GUID estável derivado de um ID textual do mobile — o mesmo seed gera sempre os mesmos IDs.</summary>
    internal static Guid IdFixo(string chave) => new(MD5.HashData(Encoding.UTF8.GetBytes($"gestao-epi:{chave}")));

    /// <summary>Port exato do gerador mulberry32 usado no seed.ts (aritmética de 32 bits sem sinal).</summary>
    private sealed class Mulberry32(uint semente)
    {
        private uint _estado = semente;

        public double Numero()
        {
            unchecked
            {
                _estado += 0x6D2B79F5;
                var t = _estado;
                t = (t ^ (t >> 15)) * (1 | t);
                t = (t + (t ^ (t >> 7)) * (61 | t)) ^ t;
                return (t ^ (t >> 14)) / 4294967296.0;
            }
        }

        public int Inteiro(int min, int maxExclusivo) => min + (int)Math.Floor(Numero() * (maxExclusivo - min));

        public T Escolher<T>(IReadOnlyList<T> lista) => lista[(int)Math.Floor(Numero() * lista.Count)];
    }
}
