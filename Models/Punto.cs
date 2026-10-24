namespace MiniCAD_OOP.Models;

public class Punto : EntitaGeometrica
{
    public Punto(int id, string nome, Colore colore, Layer layer, double x, double y)
        : base(id, nome, colore, layer)
    {
        if (!double.IsFinite(x))
        {
            throw new ArgumentOutOfRangeException(nameof(x), "La coordinata X deve essere finita.");
        }

        if (!double.IsFinite(y))
        {
            throw new ArgumentOutOfRangeException(nameof(y), "La coordinata Y deve essere finita.");
        }

        X = x;
        Y = y;
    }

    public double X { get; }

    public double Y { get; }

    public double Distanza(Punto altro)
    {
        ArgumentNullException.ThrowIfNull(altro);

        var differenzaX = Math.Abs(X - altro.X);
        var differenzaY = Math.Abs(Y - altro.Y);
        var scala = Math.Max(differenzaX, differenzaY);

        if (double.IsPositiveInfinity(scala))
        {
            return double.PositiveInfinity;
        }

        if (scala == 0)
        {
            return 0;
        }

        var xScalata = differenzaX / scala;
        var yScalata = differenzaY / scala;
        return scala * Math.Sqrt(xScalata * xScalata + yScalata * yScalata);
    }

    public override string Descrizione()
    {
        return $"Punto \"{Nome}\" (ID: {Id}) alle coordinate ({X}, {Y}), colore {Colore}, layer \"{Layer.Nome}\".";
    }
}
