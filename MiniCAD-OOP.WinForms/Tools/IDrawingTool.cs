using System.Drawing;
using MiniCAD_OOP.Models;

namespace MiniCAD_OOP.WinForms.Tools;

public interface IDrawingTool
{
    event Action<string>? StatusChanged;

    void OnMouseDown(PosizionePuntatore posizione);

    void OnMouseMove(PosizionePuntatore posizione);

    void DrawPreview(Graphics graphics, Func<CoordinateMondo, PointF> worldToScreen);

    void Cancel();
}

public interface IEntityCreatingTool
{
    event Action<EntitaGeometrica>? EntityCreated;
}

public interface IPreviewDrawingTool
{
    event Action? PreviewChanged;
}

public interface ISelectionTool
{
    event Action<EntitaGeometrica?>? SelectionChanged;
}

public interface IMouseUpDrawingTool
{
    void OnMouseUp(PosizionePuntatore posizione);
}

public interface IMouseCaptureDrawingTool
{
    event Action<bool>? MouseCaptureRequested;
}

public interface IEntityPreviewTool : IEntityMovingTool
{
    EntitaGeometrica? EntitaAnteprima { get; }
}

public interface IEntityMovingTool
{
    EntitaGeometrica? EntitaDaSpostare { get; }

    event Action<EntitaGeometrica, double, double>? MoveRequested;

    void AggiornaEntita(EntitaGeometrica? entita);
}
