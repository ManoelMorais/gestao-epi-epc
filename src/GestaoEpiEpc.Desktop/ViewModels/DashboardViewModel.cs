using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestaoEpiEpc.Application.Dtos;
using GestaoEpiEpc.Application.Services;
using GestaoEpiEpc.Desktop.Common;

namespace GestaoEpiEpc.Desktop.ViewModels;

public partial class DashboardViewModel(IDashboardService dashboardServico) : ObservableObject, IInicializavel
{
    private const double AlturaGrafico = 130;
    private const double PaddingGrafico = 14;
    private const double RaioMarcador = 3.5;
    private const double LarguraGraficoMinima = 280;
    private const double LarguraGraficoPadrao = 860;

    private IReadOnlyList<PontoSerieTemporal> _ultimaSerieEvolucao = Array.Empty<PontoSerieTemporal>();
    private double _larguraGrafico = LarguraGraficoPadrao;

    [ObservableProperty] private int totalEntregas;
    [ObservableProperty] private int totalEpiEntregues;
    [ObservableProperty] private int totalEpcMovimentados;
    [ObservableProperty] private int totalTrocas;
    [ObservableProperty] private int entregasUltimos30Dias;
    [ObservableProperty] private int totalEstornos;
    [ObservableProperty] private int colaboradoresAfastadosOuInativos;
    [ObservableProperty] private int solicitacoesEmAberto;
    [ObservableProperty] private int solicitacoesAguardandoRetirada;

    [ObservableProperty] private Geometry? evolucaoArea;
    [ObservableProperty] private Geometry? evolucaoLinha;
    [ObservableProperty] private List<Point> evolucaoMarcadores = new();
    [ObservableProperty] private List<string> evolucaoRotulosEixoX = new();
    [ObservableProperty] private int evolucaoMaximo;
    [ObservableProperty] private int evolucaoMetade;
    [ObservableProperty] private int evolucaoTotalPeriodo;
    [ObservableProperty] private string evolucaoPeriodo = string.Empty;

    public ObservableCollection<BarraIndicador> TopColaboradores { get; } = new();
    public ObservableCollection<BarraIndicador> TopItens { get; } = new();
    public ObservableCollection<BarraIndicador> EntregasPorSetor { get; } = new();
    public ObservableCollection<BarraIndicador> EntregasPorUnidade { get; } = new();
    public ObservableCollection<BarraIndicador> EntregasPorFacilitador { get; } = new();
    public ObservableCollection<BarraIndicador> EntregasPorMotivo { get; } = new();

    public async Task InicializarAsync() => await AtualizarAsync();

    /// <summary>Chamado pela view quando a área do gráfico é redimensionada, para recalcular a curva sem depender de Viewbox (evita distorcer texto/linhas).</summary>
    public void AtualizarLarguraGrafico(double largura)
    {
        if (largura < LarguraGraficoMinima || Math.Abs(largura - _larguraGrafico) < 1)
            return;

        _larguraGrafico = largura;
        PreencherEvolucao(_ultimaSerieEvolucao);
    }

    [RelayCommand]
    private async Task AtualizarAsync()
    {
        var indicadores = await dashboardServico.ObterIndicadoresAsync();

        TotalEntregas = indicadores.TotalEntregas;
        TotalEpiEntregues = indicadores.TotalEpiEntregues;
        TotalEpcMovimentados = indicadores.TotalEpcMovimentados;
        TotalTrocas = indicadores.TotalTrocas;
        EntregasUltimos30Dias = indicadores.EntregasUltimos30Dias;
        TotalEstornos = indicadores.TotalEstornos;
        SolicitacoesEmAberto = indicadores.SolicitacoesEmAberto;
        SolicitacoesAguardandoRetirada = indicadores.SolicitacoesAguardandoRetirada;
        ColaboradoresAfastadosOuInativos = indicadores.ColaboradoresAfastadosOuInativos;

        PreencherBarras(TopColaboradores, indicadores.TopColaboradores);
        PreencherBarras(TopItens, indicadores.TopItens);
        PreencherBarras(EntregasPorSetor, indicadores.EntregasPorSetor);
        PreencherBarras(EntregasPorUnidade, indicadores.EntregasPorUnidade);
        PreencherBarras(EntregasPorFacilitador, indicadores.EntregasPorFacilitador);
        PreencherBarras(EntregasPorMotivo, indicadores.EntregasPorMotivo);

        _ultimaSerieEvolucao = indicadores.EvolucaoDiaria;
        PreencherEvolucao(_ultimaSerieEvolucao);
    }

    private static void PreencherBarras(ObservableCollection<BarraIndicador> destino, IReadOnlyList<ItemContagem> origem)
    {
        destino.Clear();
        var maximo = origem.Count == 0 ? 1 : origem.Max(o => o.Quantidade);
        foreach (var item in origem)
            destino.Add(new BarraIndicador(item.Rotulo, item.Quantidade, maximo == 0 ? 0 : (double)item.Quantidade / maximo));
    }

    private void PreencherEvolucao(IReadOnlyList<PontoSerieTemporal> pontos)
    {
        if (pontos.Count == 0)
        {
            EvolucaoArea = null;
            EvolucaoLinha = null;
            EvolucaoMarcadores = new List<Point>();
            EvolucaoRotulosEixoX = new List<string>();
            EvolucaoMaximo = 0;
            EvolucaoMetade = 0;
            EvolucaoTotalPeriodo = 0;
            EvolucaoPeriodo = string.Empty;
            return;
        }

        // Escala mínima de 2 para o eixo nunca repetir rótulos (1, 0, 0) quando o período tem poucas entregas.
        var maximo = Math.Max(2, pontos.Max(p => p.Quantidade));
        var alturaUtil = AlturaGrafico - PaddingGrafico * 2;
        var passo = pontos.Count > 1 ? (_larguraGrafico - PaddingGrafico * 2) / (pontos.Count - 1) : 0;

        var coordenadas = new List<Point>(pontos.Count);
        for (var i = 0; i < pontos.Count; i++)
        {
            var x = PaddingGrafico + (pontos.Count > 1 ? i * passo : (_larguraGrafico - PaddingGrafico * 2) / 2);
            var proporcao = (double)pontos[i].Quantidade / maximo;
            var y = PaddingGrafico + (alturaUtil - proporcao * alturaUtil);
            coordenadas.Add(new Point(x, y));
        }

        var linha = CriarCurvaSuave(coordenadas);
        EvolucaoLinha = linha;

        var area = (PathFigure)linha.Figures[0].Clone();
        area.IsClosed = true;
        area.Segments.Add(new LineSegment(new Point(coordenadas[^1].X, AlturaGrafico), true));
        area.Segments.Add(new LineSegment(new Point(coordenadas[0].X, AlturaGrafico), true));
        var geometriaArea = new PathGeometry();
        geometriaArea.Figures.Add(area);
        EvolucaoArea = geometriaArea;

        EvolucaoMarcadores = coordenadas
            .Select(p => new Point(p.X - RaioMarcador, p.Y - RaioMarcador))
            .ToList();

        var qtdRotulos = Math.Min(6, pontos.Count);
        var rotulos = new List<string>(qtdRotulos);
        for (var i = 0; i < qtdRotulos; i++)
        {
            var indice = qtdRotulos == 1 ? 0 : (int)Math.Round(i * (pontos.Count - 1) / (double)(qtdRotulos - 1));
            rotulos.Add(pontos[indice].Data.ToString("dd/MM"));
        }
        EvolucaoRotulosEixoX = rotulos;

        EvolucaoMaximo = maximo;
        EvolucaoMetade = maximo / 2;
        EvolucaoTotalPeriodo = pontos.Sum(p => p.Quantidade);
        EvolucaoPeriodo = $"{pontos[0].Data:dd/MM} a {pontos[^1].Data:dd/MM}";
    }

    /// <summary>Converte uma sequência de pontos numa curva suave (Catmull-Rom convertido em Béziers cúbicas), evitando o efeito "serrote" de um Polyline ligando pontos diários.</summary>
    private static PathGeometry CriarCurvaSuave(IReadOnlyList<Point> pontos)
    {
        var figura = new PathFigure { StartPoint = pontos[0], IsClosed = false };

        if (pontos.Count < 3)
        {
            for (var i = 1; i < pontos.Count; i++)
                figura.Segments.Add(new LineSegment(pontos[i], true));
        }
        else
        {
            for (var i = 0; i < pontos.Count - 1; i++)
            {
                var p0 = pontos[Math.Max(i - 1, 0)];
                var p1 = pontos[i];
                var p2 = pontos[i + 1];
                var p3 = pontos[Math.Min(i + 2, pontos.Count - 1)];

                var controle1 = new Point(p1.X + (p2.X - p0.X) / 6.0, p1.Y + (p2.Y - p0.Y) / 6.0);
                var controle2 = new Point(p2.X - (p3.X - p1.X) / 6.0, p2.Y - (p3.Y - p1.Y) / 6.0);
                figura.Segments.Add(new BezierSegment(controle1, controle2, p2, true));
            }
        }

        var geometria = new PathGeometry();
        geometria.Figures.Add(figura);
        return geometria;
    }
}

/// <summary>Um par rótulo/valor já com a proporção calculada, pronto para virar uma barra na tela.</summary>
public record BarraIndicador(string Rotulo, int Quantidade, double Proporcao);
