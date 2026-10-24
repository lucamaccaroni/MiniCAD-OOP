namespace MiniCAD_OOP.Models;

public class Linea : EntitaGeometrica
{
    public Linea(int id, string nome, Colore colore, Layer layer, Punto inizio, Punto fine)
        : base(id, nome, colore, layer)
    {
        ArgumentNullException.ThrowIfNull(inizio);
        ArgumentNullException.ThrowIfNull(fine);

        Inizio = inizio;
        Fine = fine;
    }

    public Punto Inizio { get; }

    public Punto Fine { get; }

    public double Lunghezza()
    {
        return Inizio.Distanza(Fine);
    }

    public override string Descrizione()
    {
        return $"Linea \"{Nome}\" (ID: {Id}) da {Inizio.Nome} a {Fine.Nome}, colore {Colore}, layer \"{Layer.Nome}\".";
    }
}
