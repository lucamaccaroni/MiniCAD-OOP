using MiniCAD_OOP.Models;

namespace MiniCAD_OOP.WinForms.Tools;

public sealed class DrawingToolContext
{
    private readonly Func<int> generaId;

    public DrawingToolContext(Func<int> generaId, Layer layer, Colore colore = Colore.Blu)
    {
        ArgumentNullException.ThrowIfNull(generaId);
        ArgumentNullException.ThrowIfNull(layer);

        this.generaId = generaId;
        Layer = layer;
        Colore = colore;
    }

    public Layer Layer { get; }

    public Colore Colore { get; }

    public int GeneraId()
    {
        return generaId();
    }
}
