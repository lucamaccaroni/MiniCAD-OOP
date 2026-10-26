using System.Drawing;
using MiniCAD_OOP.Models;
using MiniCAD_OOP.Services;

namespace MiniCAD_OOP.WinForms.Tools;

public sealed class SelectionTool : IDrawingTool, ISelectionTool
{
    private const double TolleranzaPixel = 7;

    private readonly DocumentoCAD documento;
    private readonly Func<CoordinateMondo, PointF> worldToScreen;

    public SelectionTool(
        DocumentoCAD documento,
        Func<CoordinateMondo, PointF> worldToScreen)
    {
        ArgumentNullException.ThrowIfNull(documento);
        ArgumentNullException.ThrowIfNull(worldToScreen);
        this.documento = documento;
        this.worldToScreen = worldToScreen;
    }

    public event Action<string>? StatusChanged;

    public event Action<EntitaGeometrica?>? SelectionChanged;

    public void OnMouseDown(PosizionePuntatore posizione)
    {
        EntitaGeometrica? selezionata = null;
        var distanzaMinimaQuadrata = TolleranzaPixel * TolleranzaPixel;

        for (var indice = documento.Entita.Count - 1; indice >= 0; indice--)
        {
            var entita = documento.Entita[indice];
            var distanzaQuadrata = DistanzaQuadrata(entita, posizione.Schermo);
            if (distanzaQuadrata <= TolleranzaPixel * TolleranzaPixel
                && (selezionata is null || distanzaQuadrata < distanzaMinimaQuadrata))
            {
                selezionata = entita;
                distanzaMinimaQuadrata = distanzaQuadrata;
            }
        }

        SelectionChanged?.Invoke(selezionata);
        StatusChanged?.Invoke(selezionata is null
            ? "Nessuna entità selezionata."
            : $"Selezionata {selezionata.Nome}.");
    }

    public void OnMouseMove(PosizionePuntatore posizione)
    {
    }

    public void DrawPreview(
        Graphics graphics,
        Func<CoordinateMondo, PointF> worldToScreen)
    {
    }

    public void Cancel()
    {
    }

    private double DistanzaQuadrata(EntitaGeometrica entita, Point posizione)
    {
        return entita switch
        {
            Punto punto => DistanzaPuntoQuadrata(
                posizione,
                Proietta(punto.X, punto.Y)),
            Linea linea => DistanzaSegmentoQuadrata(
                posizione,
                Proietta(linea.Inizio.X, linea.Inizio.Y),
                Proietta(linea.Fine.X, linea.Fine.Y)),
            FiguraGeometrica figura => DistanzaContornoQuadrata(posizione, figura.Vertici),
            _ => double.PositiveInfinity
        };
    }

    private double DistanzaContornoQuadrata(Point posizione, IReadOnlyList<Punto> vertici)
    {
        var distanzaMinimaQuadrata = double.PositiveInfinity;

        for (var indice = 0; indice < vertici.Count; indice++)
        {
            var inizio = vertici[indice];
            var fine = vertici[(indice + 1) % vertici.Count];
            var distanzaQuadrata = DistanzaSegmentoQuadrata(
                posizione,
                Proietta(inizio.X, inizio.Y),
                Proietta(fine.X, fine.Y));
            distanzaMinimaQuadrata = Math.Min(distanzaMinimaQuadrata, distanzaQuadrata);
        }

        return distanzaMinimaQuadrata;
    }

    private PointF Proietta(double x, double y)
    {
        return worldToScreen(new CoordinateMondo(x, y));
    }

    private static double DistanzaPuntoQuadrata(Point primo, PointF secondo)
    {
        if (!float.IsFinite(secondo.X) || !float.IsFinite(secondo.Y))
        {
            return double.PositiveInfinity;
        }

        var differenzaX = (double)primo.X - secondo.X;
        var differenzaY = (double)primo.Y - secondo.Y;
        return differenzaX * differenzaX + differenzaY * differenzaY;
    }

    private static double DistanzaSegmentoQuadrata(Point punto, PointF inizio, PointF fine)
    {
        if (!float.IsFinite(inizio.X) || !float.IsFinite(inizio.Y)
            || !float.IsFinite(fine.X) || !float.IsFinite(fine.Y))
        {
            return double.PositiveInfinity;
        }

        var differenzaX = (double)fine.X - inizio.X;
        var differenzaY = (double)fine.Y - inizio.Y;
        var lunghezzaQuadrata = differenzaX * differenzaX + differenzaY * differenzaY;

        if (!double.IsFinite(lunghezzaQuadrata))
        {
            return double.PositiveInfinity;
        }

        if (lunghezzaQuadrata == 0)
        {
            return DistanzaPuntoQuadrata(punto, inizio);
        }

        var proiezione = (((double)punto.X - inizio.X) * differenzaX
            + ((double)punto.Y - inizio.Y) * differenzaY) / lunghezzaQuadrata;
        proiezione = Math.Clamp(proiezione, 0, 1);

        var puntoVicinoX = inizio.X + proiezione * differenzaX;
        var puntoVicinoY = inizio.Y + proiezione * differenzaY;
        var distanzaX = (double)punto.X - puntoVicinoX;
        var distanzaY = (double)punto.Y - puntoVicinoY;
        return distanzaX * distanzaX + distanzaY * distanzaY;
    }
}
