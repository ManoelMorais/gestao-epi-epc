using GestaoEpiEpc.Domain.Common;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Domain.Entities;

public class MotivoMovimentacao : EntidadeBase
{
    /// <summary>Identificador estável usado pelo app mobile (ex.: "mot-validade" é pré-selecionado para item vencido).</summary>
    public string Codigo { get; set; } = string.Empty;
    public required string Descricao { get; set; }
    public required TipoMovimentacao TipoAplicavel { get; set; }

    /// <summary>Não há material antigo para recolher (ex.: perda) — a solicitação não pede lote/marca/fabricação.</summary>
    public bool SemDevolucao { get; set; }
}
