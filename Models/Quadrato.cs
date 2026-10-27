namespace MiniCAD_OOP.Models;

public class Quadrato : FiguraGeometrica
{
    public Quadrato(
        int id,
        string nome,
        Colore colore,
        Layer layer,
        IEnumerable<Punto> vertici)
        : base(id, nome, colore, layer, vertici, 4)
    {
        ValidaRettangolo();
        if (!HaLatiUguali())
        {
            throw new ArgumentException("Tutti i lati del quadrato devono avere lunghezza uguale.");
        }
    }

    public override double Area()
    {
        return CalcolaArea();
    }

    public override double Perimetro()
    {
        return CalcolaPerimetro();
    }

    protected override FiguraGeometrica CreaCopiaTraslata(
        IReadOnlyList<Punto> verticiTraslati)
    {
        return new Quadrato(Id, Nome, Colore, Layer, verticiTraslati);
    }
}
