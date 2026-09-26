using CommunityToolkit.Mvvm.ComponentModel;
using GestaoEpiEpc.Domain.Entities;

namespace GestaoEpiEpc.Desktop.Common;

/// <summary>
/// Representa o usuário "logado" no desktop. Hoje é escolhido manualmente no topo da janela
/// (não há autenticação real ainda); quando o Active Directory/Entra ID for conectado, este é o
/// único lugar que muda — passa a ser preenchido pelo resultado do login, e o resto do app
/// continua lendo <see cref="UsuarioAtual"/> normalmente.
/// </summary>
public partial class SessaoAtual : ObservableObject
{
    [ObservableProperty]
    private Usuario? usuarioAtual;
}
