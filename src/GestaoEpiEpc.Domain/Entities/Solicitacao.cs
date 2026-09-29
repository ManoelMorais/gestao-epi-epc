using GestaoEpiEpc.Domain.Common;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Domain.Entities;

/// <summary>
/// Pedido de troca/reposição de um EPI/EPC feito pelo próprio colaborador no app mobile.
/// O desktop analisa, aprova ou recusa e, na retirada, registra a <see cref="Entrega"/> correspondente.
/// </summary>
public class Solicitacao : EntidadeBase
{
    /// <summary>Número exibido ao colaborador, ex.: SOL-2026-02401.</summary>
    public required string Protocolo { get; set; }

    public required Guid ColaboradorId { get; set; }
    public Colaborador? Colaborador { get; set; }

    public required Guid ItemId { get; set; }
    public ItemEpiEpc? Item { get; set; }

    public required Guid MotivoId { get; set; }
    public MotivoMovimentacao? Motivo { get; set; }

    public string? Tamanho { get; set; }
    public int Quantidade { get; set; } = 1;

    // Material antigo que está sendo substituído (bloco "material antigo" do SIGME).
    public DateOnly? MaterialDataEntrega { get; set; }
    /// <summary>Mês/ano de fabricação no formato MM/AAAA.</summary>
    public string? MaterialFabricacao { get; set; }
    public string? MaterialMarca { get; set; }
    public string? MaterialLote { get; set; }
    public required string Relato { get; set; }

    public List<string> Fotos { get; set; } = new();
    /// <summary>Caminhos SVG da assinatura desenhada no app (área de 300×120).</summary>
    public required string Assinatura { get; set; }

    public StatusSolicitacao Status { get; set; } = StatusSolicitacao.Pendente;
    public DateTime CriadaEm { get; set; } = DateTime.Now;

    /// <summary>Entrega gerada quando a solicitação foi atendida.</summary>
    public Guid? EntregaId { get; set; }
    public Entrega? Entrega { get; set; }

    public ICollection<EventoSolicitacao> Historico { get; set; } = new List<EventoSolicitacao>();
}
