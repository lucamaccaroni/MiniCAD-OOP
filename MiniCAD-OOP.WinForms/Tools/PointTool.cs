using MiniCAD_OOP.Models;

namespace MiniCAD_OOP.WinForms.Tools;

public sealed class PointTool : IDrawingTool
{
    private readonly DrawingToolContext context;

    public PointTool(DrawingToolContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        this.context = context;
    }

    public event Action<EntitaGeometrica>? EntityCreated;

    public event Action? PreviewChanged;

    public event Action<string>? StatusChanged;

    public void OnMouseDown(CoordinateMondo posizione)
    {
        var id = context.GeneraId();
        var punto = new Punto(
            id,
            $"Punto {id}",
            context.Colore,
            context.Layer,
            posizione.X,
            posizione.Y);

        EntityCreated?.Invoke(punto);
        PreviewChanged?.Invoke();
        StatusChanged?.Invoke($"Creato {punto.Nome}.");
    }

    public void OnMouseMove(CoordinateMondo posizione)
    {
    }

    public void DrawPreview(
        Graphics graphics,
        Func<CoordinateMondo, PointF> worldToScreen)
    {
    }

    public void Cancel()
    {
        StatusChanged?.Invoke("Operazione annullata.");
    }
}
