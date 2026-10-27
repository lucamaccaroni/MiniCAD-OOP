namespace MiniCAD_OOP.Models;

public abstract class FiguraGeometrica : EntitaGeometrica
{
    private const double TolleranzaDirezionale = 1e-12;
    private const double TolleranzaRelativaLati = 1e-12;

    protected FiguraGeometrica(
        int id,
        string nome,
        Colore colore,
        Layer layer,
        IEnumerable<Punto> vertici,
        int numeroVertici)
        : base(id, nome, colore, layer)
    {
        ArgumentNullException.ThrowIfNull(vertici);

        var copiaVertici = vertici.ToArray();
        if (copiaVertici.Length != numeroVertici)
        {
            throw new ArgumentException(
                $"La figura deve avere esattamente {numeroVertici} vertici.",
                nameof(vertici));
        }

        if (copiaVertici.Any(punto => punto is null))
        {
            throw new ArgumentException("I vertici non possono essere nulli.", nameof(vertici));
        }

        for (var i = 0; i < copiaVertici.Length; i++)
        {
            for (var j = i + 1; j < copiaVertici.Length; j++)
            {
                if (copiaVertici[i].X == copiaVertici[j].X
                    && copiaVertici[i].Y == copiaVertici[j].Y)
                {
                    throw new ArgumentException(
                        "I vertici devono avere coordinate distinte.",
                        nameof(vertici));
                }
            }
        }

        Vertici = Array.AsReadOnly(copiaVertici);
    }

    public IReadOnlyList<Punto> Vertici { get; }

    public abstract double Area();

    public abstract double Perimetro();

    public override FiguraGeometrica CopiaTraslata(double deltaX, double deltaY)
    {
        ValidaDeltaTraslazione(deltaX, deltaY);
        var verticiTraslati = Vertici
            .Select(punto => punto.CopiaTraslata(deltaX, deltaY))
            .ToArray();

        return CreaCopiaTraslata(verticiTraslati);
    }

    public override string Descrizione()
    {
        var vertici = string.Join(
            "; ",
            Vertici.Select(punto => $"({punto.X}, {punto.Y})"));
        return $"{GetType().Name} \"{Nome}\" (ID: {Id}) con vertici {vertici}, colore {Colore}, layer \"{Layer.Nome}\".";
    }

    protected double CalcolaArea()
    {
        var origine = Vertici[0];
        var (x, scalaX) = CoordinateScalate(origine, usaX: true);
        var (y, scalaY) = CoordinateScalate(origine, usaX: false);
        if (scalaX == 0 || scalaY == 0)
        {
            return 0;
        }

        var doppiaAreaScalata = 0d;
        for (var i = 0; i < Vertici.Count; i++)
        {
            var successivo = (i + 1) % Vertici.Count;
            doppiaAreaScalata += DifferenzaProdotti(
                x[i],
                y[successivo],
                x[successivo],
                y[i]);
        }

        return ProdottoScalato(Math.Abs(doppiaAreaScalata) / 2, scalaX, scalaY);
    }

    protected double CalcolaPerimetro()
    {
        var perimetro = 0d;

        for (var i = 0; i < Vertici.Count; i++)
        {
            perimetro += Vertici[i].Distanza(Vertici[(i + 1) % Vertici.Count]);
        }

        return perimetro;
    }

    protected void ValidaTriangolo()
    {
        var primo = Vettore(Vertici[0], Vertici[1]);
        var secondo = Vettore(Vertici[0], Vertici[2]);
        if (Math.Abs(Cross(primo, secondo)) <= TolleranzaDirezionale)
        {
            throw new ArgumentException("I vertici del triangolo non possono essere allineati.");
        }
    }

    protected void ValidaRettangolo()
    {
        var lati = new (double X, double Y)[Vertici.Count];
        for (var i = 0; i < Vertici.Count; i++)
        {
            lati[i] = Vettore(
                Vertici[i],
                Vertici[(i + 1) % Vertici.Count]);
        }

        for (var i = 0; i < lati.Length; i++)
        {
            var successivo = lati[(i + 1) % lati.Length];
            if (Math.Abs(Dot(lati[i], successivo)) > TolleranzaDirezionale)
            {
                throw new ArgumentException(
                    "I vertici devono definire angoli retti e seguire il contorno del rettangolo.");
            }
        }

        for (var i = 0; i < 2; i++)
        {
            var opposto = lati[i + 2];
            if (Math.Abs(Cross(lati[i], opposto)) > TolleranzaDirezionale
                || Dot(lati[i], opposto) >= 0)
            {
                throw new ArgumentException(
                    "I lati opposti devono essere paralleli e orientati in direzioni opposte.");
            }
        }
    }

    protected bool HaLatiUguali()
    {
        var lunghezze = new LunghezzaScalata[Vertici.Count];
        for (var i = 0; i < Vertici.Count; i++)
        {
            lunghezze[i] = LunghezzaLato(Vertici[i], Vertici[(i + 1) % Vertici.Count]);
        }

        var latoRiferimento = lunghezze[0];
        return lunghezze.All(lato => LunghezzeQuasiUguali(lato, latoRiferimento));
    }

    private (double[] Coordinate, double Scala) CoordinateScalate(Punto origine, bool usaX)
    {
        var coordinate = new double[Vertici.Count];
        var scala = 0d;
        var differenzeFinite = true;

        for (var i = 0; i < Vertici.Count; i++)
        {
            var valore = usaX ? Vertici[i].X : Vertici[i].Y;
            var valoreOrigine = usaX ? origine.X : origine.Y;
            var differenza = valore - valoreOrigine;
            coordinate[i] = differenza;
            differenzeFinite &= double.IsFinite(differenza);
            scala = Math.Max(scala, Math.Abs(differenza));
        }

        if (!differenzeFinite)
        {
            scala = Vertici.Max(punto => Math.Abs(usaX ? punto.X : punto.Y));
            for (var i = 0; i < Vertici.Count; i++)
            {
                var valore = usaX ? Vertici[i].X : Vertici[i].Y;
                var valoreOrigine = usaX ? origine.X : origine.Y;
                coordinate[i] = valore / scala - valoreOrigine / scala;
            }
        }
        else if (scala != 0)
        {
            scala = Math.ScaleB(1d, Math.ILogB(scala));
            for (var i = 0; i < coordinate.Length; i++)
            {
                coordinate[i] /= scala;
            }
        }

        return (coordinate, scala);
    }

    protected abstract FiguraGeometrica CreaCopiaTraslata(
        IReadOnlyList<Punto> verticiTraslati);

    private static double ProdottoScalato(double fattore, double scalaX, double scalaY)
    {
        var fattori = new[] { fattore, scalaX, scalaY };
        Array.Sort(fattori);
        return (fattori[0] * fattori[2]) * fattori[1];
    }

    private static double DifferenzaProdotti(double a, double b, double c, double d)
    {
        var primoProdotto = a * b;
        var secondoProdotto = c * d;
        var errorePrimo = Math.FusedMultiplyAdd(a, b, -primoProdotto);
        var erroreSecondo = Math.FusedMultiplyAdd(c, d, -secondoProdotto);
        return (primoProdotto - secondoProdotto) + (errorePrimo - erroreSecondo);
    }

    private static bool LunghezzeQuasiUguali(
        LunghezzaScalata primo,
        LunghezzaScalata secondo)
    {
        var minore = primo.Scala <= secondo.Scala ? primo : secondo;
        var maggiore = primo.Scala <= secondo.Scala ? secondo : primo;
        var rapporto = (minore.Scala / maggiore.Scala) * (minore.Fattore / maggiore.Fattore);
        return Math.Abs(rapporto - 1) <= TolleranzaRelativaLati;
    }

    private static (double X, double Y) Vettore(Punto inizio, Punto fine)
    {
        var x = fine.X - inizio.X;
        var y = fine.Y - inizio.Y;
        if (!double.IsFinite(x) || !double.IsFinite(y))
        {
            var scalaCoordinate = Math.Max(
                Math.Max(Math.Abs(inizio.X), Math.Abs(inizio.Y)),
                Math.Max(Math.Abs(fine.X), Math.Abs(fine.Y)));
            x = fine.X / scalaCoordinate - inizio.X / scalaCoordinate;
            y = fine.Y / scalaCoordinate - inizio.Y / scalaCoordinate;
        }

        var scalaVettore = Math.Max(Math.Abs(x), Math.Abs(y));
        if (!double.IsFinite(scalaVettore) || scalaVettore == 0)
        {
            throw new ArgumentException("I vertici devono definire lati non degeneri.");
        }

        x /= scalaVettore;
        y /= scalaVettore;
        var lunghezza = Math.Sqrt(x * x + y * y);
        return (x / lunghezza, y / lunghezza);
    }

    private static LunghezzaScalata LunghezzaLato(Punto inizio, Punto fine)
    {
        var x = fine.X - inizio.X;
        var y = fine.Y - inizio.Y;
        var scala = Math.Max(Math.Abs(x), Math.Abs(y));
        if (!double.IsFinite(scala))
        {
            scala = Math.Max(
                Math.Max(Math.Abs(inizio.X), Math.Abs(inizio.Y)),
                Math.Max(Math.Abs(fine.X), Math.Abs(fine.Y)));
            x = fine.X / scala - inizio.X / scala;
            y = fine.Y / scala - inizio.Y / scala;
        }

        if (scala == 0)
        {
            throw new ArgumentException("I vertici devono definire lati non degeneri.");
        }

        var scalaVettore = Math.Max(Math.Abs(x), Math.Abs(y));
        var xScalato = x / scalaVettore;
        var yScalato = y / scalaVettore;
        return new LunghezzaScalata(
            scala,
            scalaVettore * Math.Sqrt(xScalato * xScalato + yScalato * yScalato));
    }

    private static double Dot((double X, double Y) primo, (double X, double Y) secondo)
    {
        return primo.X * secondo.X + primo.Y * secondo.Y;
    }

    private static double Cross((double X, double Y) primo, (double X, double Y) secondo)
    {
        return primo.X * secondo.Y - primo.Y * secondo.X;
    }

    private readonly record struct LunghezzaScalata(double Scala, double Fattore);
}
