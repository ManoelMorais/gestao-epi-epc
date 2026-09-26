using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Abstractions;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;

namespace GestaoEpiEpc.Desktop.ViewModels;

public partial class UsuariosViewModel(IUsuarioService usuarioServico, IUnidadeRepository unidadesRepositorio) : ObservableObject, IInicializavel
{
    public ObservableCollection<Usuario> Usuarios { get; } = new();
    public ObservableCollection<Unidade> Unidades { get; } = new();
    public IReadOnlyList<PerfilUsuario> PerfisDisponiveis { get; } = Enum.GetValues<PerfilUsuario>();

    [ObservableProperty] private Usuario? usuarioSelecionado;
    [ObservableProperty] private Guid usuarioEmEdicaoId;
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private PerfilUsuario perfilSelecionado = PerfilUsuario.Facilitador;
    [ObservableProperty] private Unidade? unidadeSelecionada;
    [ObservableProperty] private string? mensagem;
    [ObservableProperty] private bool mostrarDetalhes;
    [ObservableProperty] private bool carregando;

    public void AbrirDetalhes(Usuario usuario)
    {
        UsuarioSelecionado = usuario;
        MostrarDetalhes = true;
    }

    [RelayCommand]
    private void FecharDetalhes() => MostrarDetalhes = false;

    public async Task InicializarAsync()
    {
        Unidades.Clear();
        foreach (var unidade in await unidadesRepositorio.ListarAsync())
            Unidades.Add(unidade);

        await CarregarUsuariosAsync();
    }

    private async Task CarregarUsuariosAsync()
    {
        Carregando = true;
        try
        {
            Usuarios.Clear();
            foreach (var usuario in await usuarioServico.ListarAsync())
                Usuarios.Add(usuario);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void NovoUsuario()
    {
        UsuarioEmEdicaoId = Guid.Empty;
        Nome = string.Empty;
        Email = string.Empty;
        PerfilSelecionado = PerfilUsuario.Facilitador;
        UnidadeSelecionada = Unidades.FirstOrDefault();
        Mensagem = null;
    }

    partial void OnUsuarioSelecionadoChanged(Usuario? value)
    {
        if (value is null) return;

        UsuarioEmEdicaoId = value.Id;
        Nome = value.Nome;
        Email = value.Email;
        PerfilSelecionado = value.Perfil;
        UnidadeSelecionada = Unidades.FirstOrDefault(u => u.Id == value.UnidadeId);
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Email) || UnidadeSelecionada is null)
        {
            Mensagem = "Preencha nome, e-mail e unidade antes de salvar.";
            return;
        }

        var usuario = new Usuario
        {
            Id = UsuarioEmEdicaoId == Guid.Empty ? Guid.NewGuid() : UsuarioEmEdicaoId,
            Nome = Nome.Trim(),
            Email = Email.Trim(),
            Perfil = PerfilSelecionado,
            UnidadeId = UnidadeSelecionada.Id,
            Ativo = true
        };

        await usuarioServico.SalvarAsync(usuario);
        Mensagem = "Usuário salvo.";
        NovoUsuario();
        await CarregarUsuariosAsync();
    }

    [RelayCommand]
    private async Task InativarAsync(Usuario? usuario)
    {
        if (usuario is null) return;
        await usuarioServico.InativarAsync(usuario.Id);
        MostrarDetalhes = false;
        await CarregarUsuariosAsync();
    }
}
