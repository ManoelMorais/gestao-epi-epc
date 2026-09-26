using GestaoEpiEpc.Domain.Common;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Domain.Entities;

public class MotivoMovimentacao : EntidadeBase
{
    public required string Descricao { get; set; }
    public required TipoMovimentacao TipoAplicavel { get; set; }
}
