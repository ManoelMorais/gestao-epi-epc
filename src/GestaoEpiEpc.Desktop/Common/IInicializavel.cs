namespace GestaoEpiEpc.Desktop.Common;

/// <summary>ViewModels que precisam carregar dados assíncronos ao serem exibidas implementam isto;
/// o MainViewModel chama InicializarAsync logo após a navegação.</summary>
public interface IInicializavel
{
    Task InicializarAsync();
}
