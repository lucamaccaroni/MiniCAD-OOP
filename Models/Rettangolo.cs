namespace MiniCAD_OOP.Models;

public class Rettangolo : FiguraGeometrica
{
    public Rettangolo(
        int id,
        string nome,
        Colore colore,
        Layer layer,
        IEnumerable<Punto> vertici)
        : base(id, nome, colore, layer, vertici, 4)
    {
        ValidaRettangolo();
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
        return new Rettangolo(Id, Nome, Colore, Layer, verticiTraslati);
    }
}
