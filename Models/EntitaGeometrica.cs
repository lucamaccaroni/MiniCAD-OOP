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

    public abstract string Descrizione();
}
