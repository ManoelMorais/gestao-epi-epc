namespace GestaoEpiEpc.Application.Exceptions;

/// <summary>Lançada ao tentar registrar a entrega de um item fora do perfil de elegibilidade do cargo do colaborador.</summary>
public class ItemNaoElegivelException(string itemNome, string cargoNome)
    : Exception($"O item \"{itemNome}\" não é elegível para o cargo \"{cargoNome}\".")
{
}
