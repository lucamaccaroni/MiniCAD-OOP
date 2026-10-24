using MiniCAD_OOP.Models;
using MiniCAD_OOP.Services;

var layer = new Layer("Principale");
var documento = new DocumentoCAD("Disegno dimostrativo");

var puntoInizio = new Punto(1, "Inizio", Colore.Blu, layer, 0, 0);
var puntoFine = new Punto(2, "Fine", Colore.Blu, layer, 3, 4);
var puntoRiferimento = new Punto(3, "Riferimento", Colore.Verde, layer, 2, 1);
var linea = new Linea(4, "Segmento", Colore.Rosso, layer, puntoInizio, puntoFine);

var triangolo = new Triangolo(
    5,
    "Triangolo",
    Colore.Giallo,
    layer,
    new[]
    {
        new Punto(101, "Vertice T1", Colore.Giallo, layer, 0, 0),
        new Punto(102, "Vertice T2", Colore.Giallo, layer, 4, 0),
        new Punto(103, "Vertice T3", Colore.Giallo, layer, 0, 3)
    });

var rettangolo = new Rettangolo(
    6,
    "Rettangolo",
    Colore.Ciano,
    layer,
    new[]
    {
        new Punto(201, "Vertice R1", Colore.Ciano, layer, 0, 0),
        new Punto(202, "Vertice R2", Colore.Ciano, layer, 6, 0),
        new Punto(203, "Vertice R3", Colore.Ciano, layer, 6, 3),
        new Punto(204, "Vertice R4", Colore.Ciano, layer, 0, 3)
    });

var quadrato = new Quadrato(
    7,
    "Quadrato",
    Colore.Magenta,
    layer,
    new[]
    {
        new Punto(301, "Vertice Q1", Colore.Magenta, layer, 0, 0),
        new Punto(302, "Vertice Q2", Colore.Magenta, layer, 2, 0),
        new Punto(303, "Vertice Q3", Colore.Magenta, layer, 2, 2),
        new Punto(304, "Vertice Q4", Colore.Magenta, layer, 0, 2)
    });

documento.AggiungiEntita(puntoInizio);
documento.AggiungiEntita(puntoFine);
documento.AggiungiEntita(puntoRiferimento);
documento.AggiungiEntita(linea);
documento.AggiungiEntita(triangolo);
documento.AggiungiEntita(rettangolo);
documento.AggiungiEntita(quadrato);

Console.WriteLine($"Documento: {documento.Nome}");
Console.WriteLine();
Console.WriteLine("Elenco delle entità:");
documento.StampaElenco();
Console.WriteLine($"Numero totale di entità: {documento.ContaEntita()}");
Console.WriteLine();

Console.WriteLine("Metriche delle figure:");
StampaMetriche(triangolo);
StampaMetriche(rettangolo);
StampaMetriche(quadrato);
Console.WriteLine();

Console.WriteLine("Ricerca dell'entità con ID 5:");
Console.WriteLine(documento.CercaEntita(5).Descrizione());

Console.WriteLine();
Console.WriteLine("Verifica ID duplicato:");
try
{
    documento.AggiungiEntita(new Punto(1, "ID duplicato", Colore.Nero, layer, 10, 10));
}
catch (InvalidOperationException errore)
{
    Console.WriteLine($"Entità non aggiunta: {errore.Message}");
}

Console.WriteLine();
Console.WriteLine("Eliminazione della linea con ID 4:");
var eliminata = documento.EliminaEntita(4);
Console.WriteLine(eliminata ? "Linea eliminata." : "Linea non trovata.");
Console.WriteLine($"Numero di entità dopo l'eliminazione: {documento.ContaEntita()}");

static void StampaMetriche(FiguraGeometrica figura)
{
    Console.WriteLine(
        $"{figura.Nome}: area = {figura.Area()}, perimetro = {figura.Perimetro()}");
}
