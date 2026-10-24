using System.Drawing;
using MiniCAD_OOP.Models;

namespace MiniCAD_OOP.WinForms.Tools;

public interface IDrawingTool
{
    event Action<EntitaGeometrica>? EntityCreated;

    event Action? PreviewChanged;

    event Action<string>? StatusChanged;

    void OnMouseDown(CoordinateMondo posizione);

    void OnMouseMove(CoordinateMondo posizione);

    void DrawPreview(Graphics graphics, Func<CoordinateMondo, PointF> worldToScreen);

    void Cancel();
}
