using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Desktop.Common;
using GestaoEpiEpc.Desktop.Models;
using GestaoEpiEpc.Domain.Entities;
using GestaoEpiEpc.Domain.Enums;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace GestaoEpiEpc.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject, IInicializavel
{
    private readonly IUsuarioService _usuarioServico;
    private readonly SessaoAtual _sessao;
    private readonly IServiceProvider _serviceProvider;

    private static readonly NavItem[] TodosOsItens =
    [
        new() { Titulo = "Dashboard", Icone = PackIconKind.ViewDashboard, TipoViewModel = typeof(DashboardViewModel),
            PerfisPermitidos = [PerfilUsuario.Gestao, PerfilUsuario.SegurancaTrabalho, PerfilUsuario.Rh, PerfilUsuario.Administrador] },
        new() { Titulo = "Colaboradores", Icone = PackIconKind.AccountMultiple, TipoViewModel = typeof(ColaboradoresViewModel),
            PerfisPermitidos = [PerfilUsuario.Gestao, PerfilUsuario.SegurancaTrabalho, PerfilUsuario.Rh, PerfilUsuario.Administrador] },
        new() { Titulo = "Entregas", Icone = PackIconKind.TruckDelivery, TipoViewModel = typeof(EntregasViewModel),
            PerfisPermitidos = [PerfilUsuario.Gestao, PerfilUsuario.SegurancaTrabalho, PerfilUsuario.Rh, PerfilUsuario.Administrador] },
        new() { Titulo = "Catálogo EPI/EPC", Icone = PackIconKind.ShieldCheck, TipoViewModel = typeof(CatalogoViewModel),
            PerfisPermitidos = [PerfilUsuario.Administrador] },
        new() { Titulo = "Cargos e Elegibilidade", Icone = PackIconKind.BriefcaseCheck, TipoViewModel = typeof(CargosElegibilidadeViewModel),
            PerfisPermitidos = [PerfilUsuario.Administrador] },
        new() { Titulo = "Usuários", Icone = PackIconKind.AccountCog, TipoViewModel = typeof(UsuariosViewModel),
            PerfisPermitidos = [PerfilUsuario.Administrador] },
        new() { Titulo = "Auditoria", Icone = PackIconKind.ClipboardTextClock, TipoViewModel = typeof(AuditoriaViewModel),
            PerfisPermitidos = [PerfilUsuario.Administrador, PerfilUsuario.SegurancaTrabalho] },
    ];

    public MainViewModel(IUsuarioService usuarioServico, SessaoAtual sessao, IServiceProvider serviceProvider)
    {
        _usuarioServico = usuarioServico;
        _sessao = sessao;
        _serviceProvider = serviceProvider;
    }

    public ObservableCollection<Usuario> UsuariosDisponiveis { get; } = new();
    public ObservableCollection<NavItem> ItensMenu { get; } = new();

    [ObservableProperty] private Usuario? usuarioAtual;
    [ObservableProperty] private NavItem? itemSelecionado;
    [ObservableProperty] private object? viewModelAtual;

    public async Task InicializarAsync()
    {
        var usuarios = await _usuarioServico.ListarAsync();

        UsuariosDisponiveis.Clear();
        foreach (var usuario in usuarios.Where(u => u.Perfil != PerfilUsuario.Facilitador && u.Ativo))
            UsuariosDisponiveis.Add(usuario);

        // Simula o login: em produção, isto vem da autenticação corporativa (AD/Entra ID).
        UsuarioAtual = UsuariosDisponiveis.FirstOrDefault(u => u.Perfil == PerfilUsuario.Administrador)
            ?? UsuariosDisponiveis.FirstOrDefault();
    }

    partial void OnUsuarioAtualChanged(Usuario? value)
    {
        _sessao.UsuarioAtual = value;

        ItensMenu.Clear();
        if (value is null) return;

        foreach (var item in TodosOsItens.Where(i => i.PerfisPermitidos.Contains(value.Perfil)))
            ItensMenu.Add(item);

        ItemSelecionado = ItensMenu.FirstOrDefault();
    }

    partial void OnItemSelecionadoChanged(NavItem? value)
    {
        if (value is null)
        {
            ViewModelAtual = null;
            return;
        }

        _ = NavegarParaAsync(value);
    }

    private async Task NavegarParaAsync(NavItem item)
    {
        var viewModel = _serviceProvider.GetRequiredService(item.TipoViewModel);
        ViewModelAtual = viewModel;

        if (viewModel is IInicializavel inicializavel)
            await inicializavel.InicializarAsync();
    }
}
