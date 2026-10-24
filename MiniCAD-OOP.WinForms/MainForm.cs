using MiniCAD_OOP.Models;
using MiniCAD_OOP.Services;

namespace MiniCAD_OOP.WinForms;

internal sealed class MainForm : Form
{
    private readonly DocumentoCAD documento;

    public MainForm()
    {
        Text = "MiniCAD-OOP";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 700);

        documento = CreaDocumentoDimostrativo();
        Controls.Add(new CanvasControl(documento));
    }

    private static DocumentoCAD CreaDocumentoDimostrativo()
    {
        var layer = new Layer("Principale");
        var documento = new DocumentoCAD("Disegno dimostrativo");

        var puntoInizio = new Punto(1, "Inizio", Colore.Blu, layer, -6, 0);
        var puntoFine = new Punto(2, "Fine", Colore.Blu, layer, -3, 4);
        var puntoRiferimento = new Punto(3, "Riferimento", Colore.Verde, layer, -4, 1);
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
                new Punto(201, "Vertice R1", Colore.Ciano, layer, 7, 0),
                new Punto(202, "Vertice R2", Colore.Ciano, layer, 13, 0),
                new Punto(203, "Vertice R3", Colore.Ciano, layer, 13, 3),
                new Punto(204, "Vertice R4", Colore.Ciano, layer, 7, 3)
            });
        var quadrato = new Quadrato(
            7,
            "Quadrato",
            Colore.Magenta,
            layer,
            new[]
            {
                new Punto(301, "Vertice Q1", Colore.Magenta, layer, 16, 0),
                new Punto(302, "Vertice Q2", Colore.Magenta, layer, 18, 0),
                new Punto(303, "Vertice Q3", Colore.Magenta, layer, 18, 2),
                new Punto(304, "Vertice Q4", Colore.Magenta, layer, 16, 2)
            });

        documento.AggiungiEntita(puntoInizio);
        documento.AggiungiEntita(puntoFine);
        documento.AggiungiEntita(puntoRiferimento);
        documento.AggiungiEntita(linea);
        documento.AggiungiEntita(triangolo);
        documento.AggiungiEntita(rettangolo);
        documento.AggiungiEntita(quadrato);

        return documento;
    }
}
