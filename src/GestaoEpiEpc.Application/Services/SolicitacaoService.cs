using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Exceptions;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Application.Services;

public class SolicitacaoService(
    ISolicitacaoRepository solicitacoesRepositorio,
    IColaboradorRepository colaboradoresRepositorio,
    IItemEpiEpcRepository itensRepositorio,
    IMotivoMovimentacaoRepository motivosRepositorio,
    IUsuarioRepository usuariosRepositorio,
    IElegibilidadeService elegibilidadeServico,
    IEntregaService entregaServico,
    ILogAuditoriaRepository auditoriaRepositorio) : ISolicitacaoService
{
    /// <summary>Numeração inicial dos protocolos (a primeira solicitação é a 02401), igual ao app.</summary>
    public const int SequencialInicialProtocolo = 2400;

    private static readonly StatusSolicitacao[] EmAndamento = [StatusSolicitacao.Pendente, StatusSolicitacao.EmAnalise, StatusSolicitacao.Aprovada];

    public Task<IReadOnlyList<Solicitacao>> ConsultarAsync(FiltroSolicitacoes filtro) =>
        solicitacoesRepositorio.ConsultarAsync(filtro);

    public Task<Solicitacao?> ObterAsync(Guid id) => solicitacoesRepositorio.ObterPorIdAsync(id);

    public async Task<Solicitacao> CriarAsync(Guid colaboradorId, NovaSolicitacaoInput input)
    {
        var colaborador = await colaboradoresRepositorio.ObterPorIdAsync(colaboradorId);
        if (colaborador is null || colaborador.Status == StatusColaborador.Inativo)
            throw new RegraDeNegocioException("Seu acesso está desativado.");

        var item = await itensRepositorio.ObterPorIdAsync(input.ItemId)
            ?? throw new RegraDeNegocioException("Item não encontrado.");

        if (!await elegibilidadeServico.ItemEhElegivelAsync(colaborador.CargoId, item.Id))
            throw new RegraDeNegocioException($"O item \"{item.Nome}\" não é elegível para o cargo {colaborador.Cargo?.Nome}.");

        var motivo = await motivosRepositorio.ObterPorIdAsync(input.MotivoId);
        if (motivo is null || motivo.TipoAplicavel is not (TipoMovimentacao.Troca or TipoMovimentacao.Reposicao))
            throw new RegraDeNegocioException("Selecione o motivo da troca.");

        if (input.Quantidade is < 1 or > 10) throw new RegraDeNegocioException("Quantidade inválida.");
        if (item.PossuiTamanho && string.IsNullOrWhiteSpace(input.Tamanho)) throw new RegraDeNegocioException("Informe o tamanho.");
        if (string.IsNullOrWhiteSpace(input.Relato)) throw new RegraDeNegocioException("Descreva o que aconteceu com o material antigo.");
        if (string.IsNullOrWhiteSpace(input.Assinatura)) throw new RegraDeNegocioException("A assinatura é obrigatória.");

        // Evita pedidos duplicados do mesmo item enquanto um anterior ainda está em andamento.
        var anteriores = await solicitacoesRepositorio.ConsultarAsync(new FiltroSolicitacoes { ColaboradorId = colaboradorId, Status = EmAndamento });
        if (anteriores.FirstOrDefault(s => s.ItemId == item.Id) is { } emAndamento)
            throw new RegraDeNegocioException($"Você já tem uma solicitação em andamento para este item ({emAndamento.Protocolo}).");

        var agora = DateTime.Now;
        var sequencial = Math.Max(await solicitacoesRepositorio.ObterUltimoSequencialProtocoloAsync(), SequencialInicialProtocolo) + 1;

        var solicitacao = new Solicitacao
        {
            Protocolo = FormatarProtocolo(agora.Year, sequencial),
            ColaboradorId = colaborador.Id,
            ItemId = item.Id,
            MotivoId = motivo.Id,
            Tamanho = item.PossuiTamanho ? input.Tamanho?.Trim() : null,
            Quantidade = input.Quantidade,
            Relato = input.Relato.Trim(),
            // Perda/extravio não tem material antigo para recolher.
            MaterialDataEntrega = motivo.SemDevolucao ? null : input.MaterialDataEntrega,
            MaterialFabricacao = motivo.SemDevolucao ? null : Limpar(input.MaterialFabricacao),
            MaterialMarca = motivo.SemDevolucao ? null : Limpar(input.MaterialMarca),
            MaterialLote = motivo.SemDevolucao ? null : Limpar(input.MaterialLote),
            Fotos = input.Fotos.ToList(),
            Assinatura = input.Assinatura,
            Status = StatusSolicitacao.Pendente,
            CriadaEm = agora
        };

        await solicitacoesRepositorio.AdicionarAsync(solicitacao);
        await solicitacoesRepositorio.AdicionarEventoAsync(new EventoSolicitacao
        {
            SolicitacaoId = solicitacao.Id,
            Status = StatusSolicitacao.Pendente,
            DataHora = agora,
            Responsavel = colaborador.Nome,
            Comentario = "Solicitação enviada pelo app."
        });

        return (await solicitacoesRepositorio.ObterPorIdAsync(solicitacao.Id))!;
    }

    public async Task IniciarAnaliseAsync(Guid solicitacaoId, Guid usuarioId)
    {
        var (solicitacao, usuario) = await CarregarAsync(solicitacaoId, usuarioId);
        ExigirStatus(solicitacao, "iniciar a análise", StatusSolicitacao.Pendente);

        await MudarStatusAsync(solicitacao, StatusSolicitacao.EmAnalise, usuario, Responsavel(usuario), comentario: null);
    }

    public async Task AprovarAsync(Guid solicitacaoId, Guid usuarioId, string? comentario = null)
    {
        var (solicitacao, usuario) = await CarregarAsync(solicitacaoId, usuarioId);
        ExigirStatus(solicitacao, "aprovar", StatusSolicitacao.Pendente, StatusSolicitacao.EmAnalise);

        var texto = Limpar(comentario) ?? $"Retire o item no Almoxarifado {solicitacao.Colaborador?.Unidade?.Sigla}.";
        await MudarStatusAsync(solicitacao, StatusSolicitacao.Aprovada, usuario, Responsavel(usuario), texto);
    }

    public async Task RecusarAsync(Guid solicitacaoId, Guid usuarioId, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new RegraDeNegocioException("Informe o motivo da recusa — ele é exibido ao colaborador no app.");

        var (solicitacao, usuario) = await CarregarAsync(solicitacaoId, usuarioId);
        ExigirStatus(solicitacao, "recusar", StatusSolicitacao.Pendente, StatusSolicitacao.EmAnalise);

        await MudarStatusAsync(solicitacao, StatusSolicitacao.Recusada, usuario, Responsavel(usuario), motivo.Trim());
    }

    public async Task<Entrega> RegistrarEntregaAsync(Guid solicitacaoId, Guid usuarioId, string? comentario = null)
    {
        var (solicitacao, usuario) = await CarregarAsync(solicitacaoId, usuarioId);
        ExigirStatus(solicitacao, "registrar a entrega", StatusSolicitacao.Aprovada);

        var colaborador = solicitacao.Colaborador!;
        var motivo = solicitacao.Motivo!;

        // Passa pelo mesmo EntregaService do resto do sistema: a elegibilidade é validada de novo aqui.
        var entrega = await entregaServico.RegistrarAsync(new RegistrarEntregaInput
        {
            ColaboradorId = colaborador.Id,
            FacilitadorId = usuario.Id,
            UnidadeId = colaborador.UnidadeId,
            MotivoId = motivo.Id,
            TipoMovimentacao = motivo.TipoAplicavel,
            Observacao = $"Solicitação {solicitacao.Protocolo}",
            Itens = [new RegistrarEntregaItemInput { ItemId = solicitacao.ItemId, Quantidade = solicitacao.Quantidade, Tamanho = solicitacao.Tamanho }]
        });

        solicitacao.EntregaId = entrega.Id;
        var texto = Limpar(comentario) ?? (motivo.SemDevolucao ? "Item entregue." : "Item entregue e material antigo recolhido.");
        await MudarStatusAsync(solicitacao, StatusSolicitacao.Entregue, usuario, $"Almoxarifado {colaborador.Unidade?.Sigla} — {usuario.Nome}", texto);

        return entrega;
    }

    public static string FormatarProtocolo(int ano, int sequencial) => $"SOL-{ano}-{sequencial:D5}";

    private async Task<(Solicitacao, Usuario)> CarregarAsync(Guid solicitacaoId, Guid usuarioId)
    {
        var solicitacao = await solicitacoesRepositorio.ObterPorIdAsync(solicitacaoId)
            ?? throw new RegraDeNegocioException("Solicitação não encontrada.");
        var usuario = await usuariosRepositorio.ObterPorIdAsync(usuarioId)
            ?? throw new RegraDeNegocioException("Usuário não encontrado.");
        return (solicitacao, usuario);
    }

    private static void ExigirStatus(Solicitacao solicitacao, string acao, params StatusSolicitacao[] permitidos)
    {
        if (!permitidos.Contains(solicitacao.Status))
            throw new RegraDeNegocioException($"Não é possível {acao}: a solicitação {solicitacao.Protocolo} está {TextoStatus(solicitacao.Status)}.");
    }

    private async Task MudarStatusAsync(Solicitacao solicitacao, StatusSolicitacao novo, Usuario usuario, string responsavel, string? comentario)
    {
        var anterior = solicitacao.Status;
        solicitacao.Status = novo;
        solicitacao.AtualizadoEm = DateTime.Now;

        await solicitacoesRepositorio.AtualizarAsync(solicitacao);
        await solicitacoesRepositorio.AdicionarEventoAsync(new EventoSolicitacao
        {
            SolicitacaoId = solicitacao.Id,
            Status = novo,
            Responsavel = responsavel,
            Comentario = comentario
        });
        await auditoriaRepositorio.RegistrarAsync(
            usuario.Id,
            entidade: nameof(Solicitacao),
            entidadeId: solicitacao.Id,
            acao: $"Solicitação {solicitacao.Protocolo}: {TextoStatus(novo)}",
            dadosAntes: anterior.ToString(),
            dadosDepois: novo.ToString());
    }

    private static string Responsavel(Usuario usuario) => usuario.Perfil switch
    {
        PerfilUsuario.SegurancaTrabalho => $"{usuario.Nome} (SST)",
        PerfilUsuario.Gestao => $"{usuario.Nome} (Gestão)",
        PerfilUsuario.Rh => $"{usuario.Nome} (RH)",
        PerfilUsuario.Facilitador => $"{usuario.Nome} (Almoxarifado)",
        _ => $"{usuario.Nome} (Administração)"
    };

    public static string TextoStatus(StatusSolicitacao status) => status switch
    {
        StatusSolicitacao.Pendente => "pendente",
        StatusSolicitacao.EmAnalise => "em análise",
        StatusSolicitacao.Aprovada => "aprovada",
        StatusSolicitacao.Entregue => "entregue",
        _ => "recusada"
    };

    private static string? Limpar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
