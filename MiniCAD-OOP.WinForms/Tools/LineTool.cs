using System.Drawing.Drawing2D;
using MiniCAD_OOP.Models;

namespace MiniCAD_OOP.WinForms.Tools;

public sealed class LineTool : IDrawingTool, IEntityCreatingTool, IPreviewDrawingTool
{
    private const double DistanzaMinimaMondo = 0.1;

    private readonly DrawingToolContext context;
    private CoordinateMondo? inizio;
    private CoordinateMondo anteprima;

    public LineTool(DrawingToolContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        this.context = context;
    }

    public event Action<EntitaGeometrica>? EntityCreated;

    public event Action? PreviewChanged;

    public event Action<string>? StatusChanged;

    public void OnMouseDown(PosizionePuntatore posizione)
    {
        var coordinateMondo = posizione.Mondo;
        if (!CoordinateValide(coordinateMondo))
        {
            StatusChanged?.Invoke("Coordinate non valide: X e Y devono essere valori finiti.");
            return;
        }

        if (inizio is null)
        {
            inizio = coordinateMondo;
            anteprima = coordinateMondo;
            PreviewChanged?.Invoke();
            StatusChanged?.Invoke("Linea: seleziona il secondo estremo. Premi Esc per annullare.");
            return;
        }

        if (!CoordinateValide(inizio.Value))
        {
            StatusChanged?.Invoke("Estremo iniziale non valido. Annulla la linea e riprova.");
            return;
        }

        if (DistanzaInferioreAllaSoglia(inizio.Value, coordinateMondo))
        {
            StatusChanged?.Invoke(
                $"Estremi troppo vicini (minimo {DistanzaMinimaMondo} unità mondo). Seleziona un altro punto o premi Esc.");
            return;
        }

        var idInizio = context.GeneraId();
        var puntoInizio = new Punto(
            idInizio,
            $"Estremo linea {idInizio}",
            context.Colore,
            context.Layer,
            inizio.Value.X,
            inizio.Value.Y);

        var idFine = context.GeneraId();
        var puntoFine = new Punto(
            idFine,
            $"Estremo linea {idFine}",
            context.Colore,
            context.Layer,
            coordinateMondo.X,
            coordinateMondo.Y);

        var idLinea = context.GeneraId();
        var linea = new Linea(
            idLinea,
            $"Linea {idLinea}",
            context.Colore,
            context.Layer,
            puntoInizio,
            puntoFine);

        inizio = null;
        anteprima = default;
        PreviewChanged?.Invoke();
        EntityCreated?.Invoke(linea);
        StatusChanged?.Invoke($"Creata {linea.Nome}.");
    }

    private static bool DistanzaInferioreAllaSoglia(
        CoordinateMondo primo,
        CoordinateMondo secondo)
    {
        var differenzaX = Math.Abs(secondo.X - primo.X);
        var differenzaY = Math.Abs(secondo.Y - primo.Y);
        var scala = Math.Max(differenzaX, differenzaY);

        if (double.IsPositiveInfinity(scala))
        {
            return false;
        }

        if (scala == 0)
        {
            return true;
        }

        if (scala >= DistanzaMinimaMondo)
        {
            return false;
        }

        var xScalato = differenzaX / scala;
        var yScalato = differenzaY / scala;
        var fattoreLunghezza = Math.Sqrt(xScalato * xScalato + yScalato * yScalato);
        return scala < DistanzaMinimaMondo / fattoreLunghezza;
    }

    public void OnMouseMove(PosizionePuntatore posizione)
    {
        if (inizio is null)
        {
            return;
        }

        if (!CoordinateValide(posizione.Mondo))
        {
            return;
        }

        anteprima = posizione.Mondo;
        PreviewChanged?.Invoke();
    }

    public void DrawPreview(
        Graphics graphics,
        Func<CoordinateMondo, PointF> worldToScreen)
    {
        if (inizio is null)
        {
            return;
        }

        var puntoInizio = worldToScreen(inizio.Value);
        var puntoCorrente = worldToScreen(anteprima);
        using var penna = new Pen(Color.DodgerBlue, 1.5f)
        {
            DashStyle = DashStyle.Dash
        };
        graphics.DrawLine(penna, puntoInizio, puntoCorrente);
        graphics.FillEllipse(
            Brushes.DodgerBlue,
            puntoInizio.X - 4,
            puntoInizio.Y - 4,
            8,
            8);
    }

    public void Cancel()
    {
        if (inizio is null)
        {
            return;
        }

        inizio = null;
        anteprima = default;
        PreviewChanged?.Invoke();
        StatusChanged?.Invoke("Disegno della linea annullato.");
    }

    private static bool CoordinateValide(CoordinateMondo coordinate)
    {
        return double.IsFinite(coordinate.X) && double.IsFinite(coordinate.Y);
    }
}
