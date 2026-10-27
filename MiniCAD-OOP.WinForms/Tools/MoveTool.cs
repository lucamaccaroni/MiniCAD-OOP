using System.Drawing;
using MiniCAD_OOP.Models;

namespace MiniCAD_OOP.WinForms.Tools;

public sealed class MoveTool :
    IDrawingTool,
    IMouseUpDrawingTool,
    IMouseCaptureDrawingTool,
    IEntityPreviewTool,
    IEntityMovingTool,
    IPreviewDrawingTool
{
    private EntitaGeometrica? entitaDaSpostare;
    private EntitaGeometrica? entitaAnteprima;
    private CoordinateMondo inizioTrascinamento;
    private bool trascinamentoAttivo;
    private bool errorePreviewSegnalato;

    public MoveTool(EntitaGeometrica entitaDaSpostare)
    {
        ArgumentNullException.ThrowIfNull(entitaDaSpostare);
        this.entitaDaSpostare = entitaDaSpostare;
    }

    public event Action<string>? StatusChanged;

    public event Action? PreviewChanged;

    public event Action<bool>? MouseCaptureRequested;

    public event Action<EntitaGeometrica, double, double>? MoveRequested;

    public EntitaGeometrica? EntitaDaSpostare => entitaDaSpostare;

    public EntitaGeometrica? EntitaAnteprima => entitaAnteprima;

    public void OnMouseDown(PosizionePuntatore posizione)
    {
        if (trascinamentoAttivo)
        {
            return;
        }

        if (entitaDaSpostare is null)
        {
            StatusChanged?.Invoke("Seleziona un'entità prima di spostarla.");
            return;
        }

        if (!CoordinateValide(posizione.Mondo))
        {
            StatusChanged?.Invoke("Coordinate non valide: lo spostamento non è iniziato.");
            return;
        }

        inizioTrascinamento = posizione.Mondo;
        trascinamentoAttivo = true;
        errorePreviewSegnalato = false;
        MouseCaptureRequested?.Invoke(true);
        StatusChanged?.Invoke($"Sposta {entitaDaSpostare.Nome}: trascina e rilascia, oppure premi Esc.");
    }

    public void OnMouseMove(PosizionePuntatore posizione)
    {
        if (!trascinamentoAttivo || entitaDaSpostare is null)
        {
            return;
        }

        if (!CoordinateValide(posizione.Mondo))
        {
            AggiornaAnteprima(null);
            SegnalaErrorePreview("Coordinate non valide: la preview è stata rimossa.");
            return;
        }

        var deltaX = posizione.Mondo.X - inizioTrascinamento.X;
        var deltaY = posizione.Mondo.Y - inizioTrascinamento.Y;
        if (!double.IsFinite(deltaX) || !double.IsFinite(deltaY))
        {
            AggiornaAnteprima(null);
            SegnalaErrorePreview("Spostamento non valido: la preview è stata rimossa.");
            return;
        }

        if (deltaX == 0 && deltaY == 0)
        {
            errorePreviewSegnalato = false;
            AggiornaAnteprima(null);
            return;
        }

        try
        {
            var nuovaAnteprima = entitaDaSpostare.CopiaTraslata(deltaX, deltaY);
            errorePreviewSegnalato = false;
            AggiornaAnteprima(nuovaAnteprima);
        }
        catch (ArgumentException errore)
        {
            AggiornaAnteprima(null);
            SegnalaErrorePreview($"Spostamento non valido: {errore.Message}");
        }
    }

    public void OnMouseUp(PosizionePuntatore posizione)
    {
        if (!trascinamentoAttivo)
        {
            return;
        }

        var origine = entitaDaSpostare;
        var deltaX = posizione.Mondo.X - inizioTrascinamento.X;
        var deltaY = posizione.Mondo.Y - inizioTrascinamento.Y;
        var deltaValido = origine is not null
            && CoordinateValide(posizione.Mondo)
            && double.IsFinite(deltaX)
            && double.IsFinite(deltaY);

        try
        {
            TerminaTrascinamento();

            if (!deltaValido || origine is null)
            {
                StatusChanged?.Invoke("Spostamento annullato: coordinate non valide.");
                return;
            }

            if (deltaX == 0 && deltaY == 0)
            {
                StatusChanged?.Invoke("Nessuno spostamento applicato.");
                return;
            }

            MoveRequested?.Invoke(origine, deltaX, deltaY);
        }
        finally
        {
            MouseCaptureRequested?.Invoke(false);
        }
    }

    public void DrawPreview(
        Graphics graphics,
        Func<CoordinateMondo, PointF> worldToScreen)
    {
    }

    public void Cancel()
    {
        if (!trascinamentoAttivo)
        {
            return;
        }

        try
        {
            TerminaTrascinamento();
            StatusChanged?.Invoke("Spostamento annullato.");
        }
        finally
        {
            MouseCaptureRequested?.Invoke(false);
        }
    }

    public void AggiornaEntita(EntitaGeometrica? entita)
    {
        if (trascinamentoAttivo)
        {
            Cancel();
        }

        entitaDaSpostare = entita;
    }

    private void TerminaTrascinamento()
    {
        trascinamentoAttivo = false;
        errorePreviewSegnalato = false;
        inizioTrascinamento = default;
        AggiornaAnteprima(null);
    }

    private void AggiornaAnteprima(EntitaGeometrica? anteprima)
    {
        if (ReferenceEquals(entitaAnteprima, anteprima))
        {
            return;
        }

        entitaAnteprima = anteprima;
        PreviewChanged?.Invoke();
    }

    private void SegnalaErrorePreview(string messaggio)
    {
        if (errorePreviewSegnalato)
        {
            return;
        }

        errorePreviewSegnalato = true;
        StatusChanged?.Invoke(messaggio);
    }

    private static bool CoordinateValide(CoordinateMondo coordinate)
    {
        return double.IsFinite(coordinate.X) && double.IsFinite(coordinate.Y);
    }
}
