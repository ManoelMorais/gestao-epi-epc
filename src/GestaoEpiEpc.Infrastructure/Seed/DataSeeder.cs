using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;
using GestaoEpiEpc.Infrastructure.InMemory;

namespace GestaoEpiEpc.Infrastructure.Seed;

/// <summary>
/// Dados de exemplo só para o sistema ser demonstrável antes de haver um banco real conectado.
/// Nada aqui sobrevive a um reinício da aplicação. O volume é proposital — o objetivo é que o
/// dashboard, os filtros e as listas tenham dado real o suficiente para uma demonstração.
/// </summary>
internal static class DataSeeder
{
    public static void Popular(InMemoryDataStore store)
    {
        // ---- Unidades ----------------------------------------------------
        var ese = new Unidade { Nome = "Amperion Sul", Sigla = "APS" };
        var emt = new Unidade { Nome = "Amperion Norte", Sigla = "APN" };
        store.Unidades.AddRange([ese, emt]);

        // ---- Cargos --------------------------------------------------------
        var eletricista = new Cargo { Nome = "Eletricista de Rede" };
        var auxiliarAlmoxarifado = new Cargo { Nome = "Auxiliar de Almoxarifado" };
        var tecnicoSst = new Cargo { Nome = "Técnico de Segurança do Trabalho" };
        var analistaAdm = new Cargo { Nome = "Analista Administrativo" };
        var supervisorCampo = new Cargo { Nome = "Supervisor de Campo" };
        var tecnicoManutencao = new Cargo { Nome = "Técnico de Manutenção" };
        store.Cargos.AddRange([eletricista, auxiliarAlmoxarifado, tecnicoSst, analistaAdm, supervisorCampo, tecnicoManutencao]);

        // ---- Categorias de item ---------------------------------------------
        var catCabeca = new CategoriaItem { Nome = "Proteção da Cabeça", Tipo = TipoItem.Epi };
        var catMaos = new CategoriaItem { Nome = "Proteção das Mãos", Tipo = TipoItem.Epi };
        var catQuedas = new CategoriaItem { Nome = "Proteção contra Quedas", Tipo = TipoItem.Epi };
        var catVestimenta = new CategoriaItem { Nome = "Vestimenta", Tipo = TipoItem.Epi };
        var catAuditiva = new CategoriaItem { Nome = "Proteção Auditiva", Tipo = TipoItem.Epi };
        var catPes = new CategoriaItem { Nome = "Proteção dos Pés", Tipo = TipoItem.Epi };
        var catSinalizacao = new CategoriaItem { Nome = "Sinalização e Isolamento", Tipo = TipoItem.Epc };
        store.CategoriasItem.AddRange([catCabeca, catMaos, catQuedas, catVestimenta, catAuditiva, catPes, catSinalizacao]);

        // ---- Catálogo de EPI/EPC ------------------------------------------
        var capacete = new ItemEpiEpc { Codigo = "EPI-001", Nome = "Capacete de Segurança Classe B", CategoriaId = catCabeca.Id, NumeroCa = "31469", ValidadePadraoMeses = 60, PossuiTamanho = false };
        var oculos = new ItemEpiEpc { Codigo = "EPI-002", Nome = "Óculos de Proteção Incolor", CategoriaId = catCabeca.Id, NumeroCa = "25763", ValidadePadraoMeses = 24, PossuiTamanho = false };
        var luvaIsolante = new ItemEpiEpc { Codigo = "EPI-003", Nome = "Luva Isolante de Borracha Classe 2", CategoriaId = catMaos.Id, NumeroCa = "28871", ValidadePadraoMeses = 12, PossuiTamanho = true };
        var cintoParaquedista = new ItemEpiEpc { Codigo = "EPI-004", Nome = "Cinto de Segurança Tipo Paraquedista", CategoriaId = catQuedas.Id, NumeroCa = "34115", ValidadePadraoMeses = 24, PossuiTamanho = true };
        var uniformeNomex = new ItemEpiEpc { Codigo = "EPI-005", Nome = "Uniforme NOMEX Manga Longa", CategoriaId = catVestimenta.Id, NumeroCa = "30044", ValidadePadraoMeses = 24, PossuiTamanho = true };
        var luvaRaspa = new ItemEpiEpc { Codigo = "EPI-006", Nome = "Luva de Raspa Reforçada", CategoriaId = catMaos.Id, NumeroCa = "19207", ValidadePadraoMeses = 6, PossuiTamanho = true };
        var talabarte = new ItemEpiEpc { Codigo = "EPI-007", Nome = "Talabarte Duplo em Y", CategoriaId = catQuedas.Id, NumeroCa = "37788", ValidadePadraoMeses = 24, PossuiTamanho = false };
        var protetorAuricular = new ItemEpiEpc { Codigo = "EPI-008", Nome = "Protetor Auricular Tipo Plug", CategoriaId = catAuditiva.Id, NumeroCa = "5745", ValidadePadraoMeses = 6, PossuiTamanho = false };
        var botaSeguranca = new ItemEpiEpc { Codigo = "EPI-009", Nome = "Bota de Segurança com Bico de Aço", CategoriaId = catPes.Id, NumeroCa = "40311", ValidadePadraoMeses = 12, PossuiTamanho = true };
        var fitaZebrada = new ItemEpiEpc { Codigo = "EPC-001", Nome = "Fita Zebrada de Sinalização", CategoriaId = catSinalizacao.Id, NumeroCa = null, ValidadePadraoMeses = null, PossuiTamanho = false };
        var coneSinalizacao = new ItemEpiEpc { Codigo = "EPC-002", Nome = "Cone de Sinalização", CategoriaId = catSinalizacao.Id, NumeroCa = null, ValidadePadraoMeses = null, PossuiTamanho = false };
        var sinalizadorLuminoso = new ItemEpiEpc { Codigo = "EPC-003", Nome = "Sinalizador Luminoso Portátil", CategoriaId = catSinalizacao.Id, NumeroCa = null, ValidadePadraoMeses = null, PossuiTamanho = false };
        store.Itens.AddRange([capacete, oculos, luvaIsolante, cintoParaquedista, uniformeNomex, luvaRaspa, talabarte, protetorAuricular, botaSeguranca, fitaZebrada, coneSinalizacao, sinalizadorLuminoso]);

        // ---- Elegibilidade por cargo ----------------------------------------
        // Eletricista de Rede: acesso ao catálogo completo de campo elétrico.
        var itensEletricista = AdicionarPermissoes(store, eletricista, capacete, oculos, luvaIsolante, cintoParaquedista, uniformeNomex, talabarte, botaSeguranca, fitaZebrada, coneSinalizacao);
        // Auxiliar de Almoxarifado: proteção básica de movimentação de materiais.
        var itensAuxiliar = AdicionarPermissoes(store, auxiliarAlmoxarifado, capacete, oculos, luvaRaspa, botaSeguranca);
        // Técnico de Segurança do Trabalho: itens de inspeção e sinalização de campo.
        var itensSst = AdicionarPermissoes(store, tecnicoSst, capacete, oculos, protetorAuricular, fitaZebrada, coneSinalizacao, sinalizadorLuminoso);
        // Analista Administrativo: função de escritório — nenhum EPI/EPC de campo é elegível.
        var itensAdm = AdicionarPermissoes(store, analistaAdm);
        // Supervisor de Campo: kit de supervisão em campo elétrico, sem itens de altura.
        var itensSupervisor = AdicionarPermissoes(store, supervisorCampo, capacete, oculos, luvaIsolante, uniformeNomex, botaSeguranca, coneSinalizacao);
        // Técnico de Manutenção: kit de manutenção, com proteção auditiva e contra quedas.
        var itensManutencao = AdicionarPermissoes(store, tecnicoManutencao, capacete, oculos, luvaRaspa, protetorAuricular, cintoParaquedista, talabarte, botaSeguranca);

        var elegibilidadePorCargo = new Dictionary<Guid, List<ItemEpiEpc>>
        {
            [eletricista.Id] = itensEletricista,
            [auxiliarAlmoxarifado.Id] = itensAuxiliar,
            [tecnicoSst.Id] = itensSst,
            [analistaAdm.Id] = itensAdm,
            [supervisorCampo.Id] = itensSupervisor,
            [tecnicoManutencao.Id] = itensManutencao,
        };

        // ---- Motivos de movimentação ----------------------------------------
        var motivoNovoColaborador = new MotivoMovimentacao { Descricao = "Novo colaborador", TipoAplicavel = TipoMovimentacao.EntregaInicial };
        var motivoDesgaste = new MotivoMovimentacao { Descricao = "Desgaste natural", TipoAplicavel = TipoMovimentacao.Reposicao };
        var motivoPerda = new MotivoMovimentacao { Descricao = "Perda", TipoAplicavel = TipoMovimentacao.Reposicao };
        var motivoValidade = new MotivoMovimentacao { Descricao = "Validade vencida", TipoAplicavel = TipoMovimentacao.Reposicao };
        var motivoTransferencia = new MotivoMovimentacao { Descricao = "Transferência entre polos", TipoAplicavel = TipoMovimentacao.Troca };
        var motivoDesligamento = new MotivoMovimentacao { Descricao = "Devolução por desligamento", TipoAplicavel = TipoMovimentacao.Devolucao };
        store.MotivosMovimentacao.AddRange([motivoNovoColaborador, motivoDesgaste, motivoPerda, motivoValidade, motivoTransferencia, motivoDesligamento]);
        var motivosReposicao = new[] { motivoDesgaste, motivoPerda, motivoValidade };

        // ---- Usuários do sistema (desktop) -----------------------------------
        var admin = new Usuario { Nome = "Manoel Almeida de Morais", Email = "manoel.morais@amperion.com.br", Perfil = PerfilUsuario.Administrador, UnidadeId = ese.Id };
        var gestao = new Usuario { Nome = "Ana Beatriz Souza", Email = "ana.souza@amperion.com.br", Perfil = PerfilUsuario.Gestao, UnidadeId = ese.Id };
        var sst = new Usuario { Nome = "Carlos Eduardo Lima", Email = "carlos.lima@amperion.com.br", Perfil = PerfilUsuario.SegurancaTrabalho, UnidadeId = ese.Id };
        var rh = new Usuario { Nome = "Fernanda Rocha", Email = "fernanda.rocha@amperion.com.br", Perfil = PerfilUsuario.Rh, UnidadeId = ese.Id };
        var facilitadorSul = new Usuario { Nome = "João Pedro Santos", Email = "joao.santos@amperion.com.br", Perfil = PerfilUsuario.Facilitador, UnidadeId = ese.Id };
        var facilitadorNorte = new Usuario { Nome = "Bruna Martins Souza", Email = "bruna.souza@amperion.com.br", Perfil = PerfilUsuario.Facilitador, UnidadeId = emt.Id };
        store.Usuarios.AddRange([admin, gestao, sst, rh, facilitadorSul, facilitadorNorte]);

        // ---- Colaboradores ---------------------------------------------------
        var roberto = new Colaborador { Matricula = "10234", Nome = "Roberto Carlos Nascimento", CargoId = eletricista.Id, Area = "Manutenção de Rede", UnidadeId = ese.Id };
        var juliana = new Colaborador { Matricula = "10567", Nome = "Juliana Alves Pereira", CargoId = eletricista.Id, Area = "Manutenção de Rede", UnidadeId = ese.Id };
        var marcos = new Colaborador { Matricula = "10890", Nome = "Marcos Vinícius Teixeira", CargoId = auxiliarAlmoxarifado.Id, Area = "Almoxarifado", UnidadeId = ese.Id };
        var patricia = new Colaborador { Matricula = "11023", Nome = "Patrícia Gomes Ferreira", CargoId = tecnicoSst.Id, Area = "Segurança do Trabalho", UnidadeId = ese.Id };
        var rafael = new Colaborador { Matricula = "11345", Nome = "Rafael Costa Andrade", CargoId = analistaAdm.Id, Area = "Administrativo", UnidadeId = ese.Id };
        var camila = new Colaborador { Matricula = "11789", Nome = "Camila Souza Ribeiro", CargoId = eletricista.Id, Area = "Manutenção de Rede", UnidadeId = emt.Id };
        var fernando = new Colaborador { Matricula = "12012", Nome = "Fernando Henrique Lopes", CargoId = supervisorCampo.Id, Area = "Manutenção de Rede", UnidadeId = ese.Id };
        var bianca = new Colaborador { Matricula = "12045", Nome = "Bianca Oliveira Cardoso", CargoId = tecnicoManutencao.Id, Area = "Manutenção de Rede", UnidadeId = ese.Id };
        var diego = new Colaborador { Matricula = "12078", Nome = "Diego Martins Rocha", CargoId = eletricista.Id, Area = "Manutenção de Rede", UnidadeId = ese.Id, Status = StatusColaborador.Afastado };
        var larissa = new Colaborador { Matricula = "12101", Nome = "Larissa Fernandes Melo", CargoId = auxiliarAlmoxarifado.Id, Area = "Almoxarifado", UnidadeId = ese.Id, Status = StatusColaborador.Inativo };
        var eduardo = new Colaborador { Matricula = "12134", Nome = "Eduardo Pereira Gomes", CargoId = tecnicoManutencao.Id, Area = "Manutenção de Rede", UnidadeId = ese.Id };
        var vanessa = new Colaborador { Matricula = "12167", Nome = "Vanessa Almeida Torres", CargoId = analistaAdm.Id, Area = "Administrativo", UnidadeId = ese.Id };
        var gustavo = new Colaborador { Matricula = "12190", Nome = "Gustavo Henrique Silva", CargoId = eletricista.Id, Area = "Manutenção de Rede", UnidadeId = emt.Id };
        var renata = new Colaborador { Matricula = "12223", Nome = "Renata Cristina Barbosa", CargoId = supervisorCampo.Id, Area = "Manutenção de Rede", UnidadeId = emt.Id };
        var thiago = new Colaborador { Matricula = "12256", Nome = "Thiago Souza Martins", CargoId = tecnicoSst.Id, Area = "Segurança do Trabalho", UnidadeId = emt.Id };
        var amanda = new Colaborador { Matricula = "12289", Nome = "Amanda Ribeiro Costa", CargoId = auxiliarAlmoxarifado.Id, Area = "Almoxarifado", UnidadeId = emt.Id };
        var bruno = new Colaborador { Matricula = "12312", Nome = "Bruno César Andrade", CargoId = tecnicoManutencao.Id, Area = "Manutenção de Rede", UnidadeId = emt.Id };
        var isabela = new Colaborador { Matricula = "12345", Nome = "Isabela Nunes Carvalho", CargoId = eletricista.Id, Area = "Manutenção de Rede", UnidadeId = emt.Id };
        store.Colaboradores.AddRange([roberto, juliana, marcos, patricia, rafael, camila, fernando, bianca, diego, larissa, eduardo, vanessa, gustavo, renata, thiago, amanda, bruno, isabela]);

        // ---- Entregas ---------------------------------------------------------
        var hoje = DateTime.Now;

        Unidade UnidadeDe(Colaborador c) => c.UnidadeId == ese.Id ? ese : emt;
        Usuario FacilitadorDe(Colaborador c) => c.UnidadeId == ese.Id ? facilitadorSul : facilitadorNorte;

        // Entregas iniciais curadas — cobrem a chegada de cada colaborador ativo no sistema.
        Entregar(store, roberto, facilitadorSul, ese, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-165), (capacete, 1), (luvaIsolante, 2), (cintoParaquedista, 1), (uniformeNomex, 2));
        Entregar(store, juliana, facilitadorSul, ese, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-158), (capacete, 1), (luvaIsolante, 2), (uniformeNomex, 2));
        Entregar(store, marcos, facilitadorSul, ese, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-150), (capacete, 1), (oculos, 1), (botaSeguranca, 1));
        Entregar(store, patricia, facilitadorSul, ese, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-142), (capacete, 1), (oculos, 1), (coneSinalizacao, 4), (protetorAuricular, 2));
        Entregar(store, camila, facilitadorNorte, emt, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-138), (capacete, 1), (luvaIsolante, 2), (cintoParaquedista, 1));
        Entregar(store, fernando, facilitadorSul, ese, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-120), (capacete, 1), (oculos, 1), (uniformeNomex, 2), (botaSeguranca, 1));
        Entregar(store, bianca, facilitadorSul, ese, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-114), (capacete, 1), (luvaRaspa, 2), (protetorAuricular, 2), (botaSeguranca, 1));
        Entregar(store, diego, facilitadorSul, ese, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-300), (capacete, 1), (luvaIsolante, 2), (uniformeNomex, 2));
        Entregar(store, larissa, facilitadorSul, ese, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-280), (capacete, 1), (oculos, 1));
        Entregar(store, eduardo, facilitadorSul, ese, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-96), (capacete, 1), (luvaRaspa, 2), (cintoParaquedista, 1));
        Entregar(store, gustavo, facilitadorNorte, emt, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-110), (capacete, 1), (luvaIsolante, 2), (uniformeNomex, 2), (botaSeguranca, 1));
        Entregar(store, renata, facilitadorNorte, emt, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-90), (capacete, 1), (oculos, 1), (uniformeNomex, 2), (botaSeguranca, 1));
        Entregar(store, thiago, facilitadorNorte, emt, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-84), (capacete, 1), (oculos, 1), (sinalizadorLuminoso, 2));
        Entregar(store, amanda, facilitadorNorte, emt, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-70), (capacete, 1), (botaSeguranca, 1));
        Entregar(store, bruno, facilitadorNorte, emt, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-60), (capacete, 1), (luvaRaspa, 2), (protetorAuricular, 2));
        Entregar(store, isabela, facilitadorNorte, emt, motivoNovoColaborador, TipoMovimentacao.EntregaInicial, hoje.AddDays(-50), (capacete, 1), (luvaIsolante, 2), (talabarte, 1));

        // Trocas — item antigo substituído, com motivo de transferência entre polos.
        Entregar(store, camila, facilitadorNorte, emt, motivoTransferencia, TipoMovimentacao.Troca, hoje.AddDays(-32), (uniformeNomex, 2));
        Entregar(store, fernando, facilitadorSul, ese, motivoTransferencia, TipoMovimentacao.Troca, hoje.AddDays(-18), (botaSeguranca, 1));
        Entregar(store, renata, facilitadorNorte, emt, motivoTransferencia, TipoMovimentacao.Troca, hoje.AddDays(-9), (uniformeNomex, 2));

        // Devolução por desligamento — Diego está afastado, Larissa inativa.
        Entregar(store, diego, facilitadorSul, ese, motivoDesligamento, TipoMovimentacao.Devolucao, hoje.AddDays(-40), (uniformeNomex, 2));
        Entregar(store, larissa, facilitadorSul, ese, motivoDesligamento, TipoMovimentacao.Devolucao, hoje.AddDays(-200), (oculos, 1));

        // Reposições geradas — volume de histórico para o dashboard e os filtros, sempre
        // respeitando a elegibilidade por cargo de cada colaborador.
        var colaboradoresAtivos = new[]
        {
            roberto, juliana, marcos, patricia, camila, fernando, bianca, eduardo,
            gustavo, renata, thiago, amanda, bruno, isabela
        };
        var aleatorio = new Random(42);

        for (var i = 0; i < 42; i++)
        {
            var colaborador = colaboradoresAtivos[aleatorio.Next(colaboradoresAtivos.Length)];
            var itensDoCargo = elegibilidadePorCargo[colaborador.CargoId];
            if (itensDoCargo.Count == 0) continue;

            var item = itensDoCargo[aleatorio.Next(itensDoCargo.Count)];
            var motivo = motivosReposicao[aleatorio.Next(motivosReposicao.Length)];
            var quantidade = aleatorio.Next(1, 3);
            var dias = aleatorio.Next(2, 130);
            var dataHora = hoje.AddDays(-dias).AddHours(-aleatorio.Next(1, 9)).AddMinutes(-aleatorio.Next(0, 59));

            Entregar(store, colaborador, FacilitadorDe(colaborador), UnidadeDe(colaborador), motivo, TipoMovimentacao.Reposicao, dataHora, (item, quantidade));
        }

        // Uma entrega já estornada, para demonstrar o fluxo de estorno no histórico.
        var ultimaEntrega = Entregar(store, juliana, facilitadorSul, ese, motivoDesgaste, TipoMovimentacao.Reposicao, hoje.AddDays(-1), (luvaIsolante, 2));
        ultimaEntrega.Status = StatusEntrega.Estornada;
        ultimaEntrega.Observacao = "Estornada: item devolvido por engano no registro.";
        store.LogsAuditoria.Add(new LogAuditoria
        {
            UsuarioId = admin.Id,
            Entidade = nameof(Entrega),
            EntidadeId = ultimaEntrega.Id,
            Acao = "Entrega estornada",
            DadosAntes = nameof(StatusEntrega.Confirmada),
            DadosDepois = nameof(StatusEntrega.Estornada)
        });
    }

    private static List<ItemEpiEpc> AdicionarPermissoes(InMemoryDataStore store, Cargo cargo, params ItemEpiEpc[] itens)
    {
        foreach (var item in itens)
        {
            store.CargoItemPermitidos.Add(new CargoItemPermitido { CargoId = cargo.Id, ItemId = item.Id });
        }

        return itens.ToList();
    }

    private static Entrega Entregar(
        InMemoryDataStore store,
        Colaborador colaborador,
        Usuario facilitador,
        Unidade unidade,
        MotivoMovimentacao motivo,
        TipoMovimentacao tipo,
        DateTime dataHora,
        params (ItemEpiEpc Item, int Quantidade)[] itens)
    {
        var entrega = new Entrega
        {
            ColaboradorId = colaborador.Id,
            FacilitadorId = facilitador.Id,
            UnidadeId = unidade.Id,
            MotivoId = motivo.Id,
            TipoMovimentacao = tipo,
            DataHora = dataHora,
            AssinaturaUrl = "assinatura-demo.png"
        };

        entrega.Itens = itens.Select(i => new EntregaItem
        {
            EntregaId = entrega.Id,
            ItemId = i.Item.Id,
            Quantidade = i.Quantidade
        }).ToList();

        store.Entregas.Add(entrega);
        return entrega;
    }
}
