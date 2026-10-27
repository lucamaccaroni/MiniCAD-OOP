namespace MiniCAD_OOP.Models;

public class Triangolo : FiguraGeometrica
{
    public Triangolo(
        int id,
        string nome,
        Colore colore,
        Layer layer,
        IEnumerable<Punto> vertici)
        : base(id, nome, colore, layer, vertici, 3)
    {
        ValidaTriangolo();
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
        return new Triangolo(Id, Nome, Colore, Layer, verticiTraslati);
    }
}
