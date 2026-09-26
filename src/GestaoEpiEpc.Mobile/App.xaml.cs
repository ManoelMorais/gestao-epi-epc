namespace GestaoEpiEpc.Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly AppShell _shell;

    public App(AppShell shell)
    {
        InitializeComponent();
        _shell = shell;

        // O app (como o desktop) tem um único design claro — não adapta ao tema escuro do SO,
        // então fixamos o tema para os cartões com fundo branco não ficarem com texto branco em cima.
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(_shell);
}
