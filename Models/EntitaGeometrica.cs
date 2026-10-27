namespace MiniCAD_OOP.Models;

public abstract class EntitaGeometrica
{
    protected EntitaGeometrica(int id, string nome, Colore colore, Layer layer)
    {
        if (id < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "L'ID non può essere negativo.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentNullException.ThrowIfNull(layer);

        if (!Enum.IsDefined(colore))
        {
            throw new ArgumentOutOfRangeException(nameof(colore), "Il colore specificato non è valido.");
        }

        Id = id;
        Nome = nome;
        Colore = colore;
        Layer = layer;
    }

    public int Id { get; }

    public string Nome { get; }

    public Colore Colore { get; }

    public Layer Layer { get; }

    public abstract EntitaGeometrica CopiaTraslata(double deltaX, double deltaY);

    public abstract string Descrizione();

    protected static void ValidaDeltaTraslazione(double deltaX, double deltaY)
    {
        if (!double.IsFinite(deltaX))
        {
            throw new ArgumentOutOfRangeException(nameof(deltaX), "Il delta X deve essere finito.");
        }

        if (!double.IsFinite(deltaY))
        {
            throw new ArgumentOutOfRangeException(nameof(deltaY), "Il delta Y deve essere finito.");
        }
    }
}
