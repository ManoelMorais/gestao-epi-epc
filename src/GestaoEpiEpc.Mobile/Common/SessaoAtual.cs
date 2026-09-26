using CommunityToolkit.Mvvm.ComponentModel;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Mobile.Common;

/// <summary>
/// Representa o facilitador "logado" no aparelho. Hoje é escolhido manualmente (não há
/// autenticação real ainda); quando o login de verdade for conectado, este é o único lugar que
/// muda — passa a ser preenchido pelo resultado do login, e o resto do app continua lendo
/// <see cref="FacilitadorAtual"/> normalmente.
/// </summary>
public partial class SessaoAtual : ObservableObject
{
    [ObservableProperty]
    private Usuario? facilitadorAtual;
}
