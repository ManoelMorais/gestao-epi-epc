using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GestaoEpiEpc.Desktop.ViewModels;

namespace GestaoEpiEpc.Desktop.Views;

public partial class SolicitacoesView : UserControl
{
    public SolicitacoesView() => InitializeComponent();

    private SolicitacoesViewModel? ViewModel => DataContext as SolicitacoesViewModel;

    private void Busca_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ViewModel?.BuscarCommand.Execute(null);
    }

    private async void Aprovar_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { SolicitacaoSelecionada: { } s } vm) return;

        var comentario = DialogoTextoWindow.Pedir(Window.GetWindow(this), $"Aprovar {s.Protocolo}",
            "Observação opcional, exibida ao colaborador no app. Em branco, ele recebe: \"Retire o item no almoxarifado da sua unidade\".",
            "Aprovar", obrigatorio: false);
        if (comentario is not null)
            await vm.AprovarAsync(comentario);
    }

    private async void Recusar_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { SolicitacaoSelecionada: { } s } vm) return;

        var motivo = DialogoTextoWindow.Pedir(Window.GetWindow(this), $"Recusar {s.Protocolo}",
            "Explique o motivo da recusa — o colaborador vê este texto no app.",
            "Recusar", obrigatorio: true, perigo: true);
        if (motivo is not null)
            await vm.RecusarAsync(motivo);
    }

    private async void RegistrarEntrega_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is not { SolicitacaoSelecionada: { } s } vm) return;

        var confirmacao = MessageBox.Show(Window.GetWindow(this),
            $"Confirmar que {s.Colaborador?.Nome} retirou {s.Quantidade}x {s.Item?.Nome}?\n\nA entrega será registrada no histórico do colaborador.",
            "Registrar retirada", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirmacao == MessageBoxResult.Yes)
            await vm.RegistrarEntregaAsync();
    }
}
