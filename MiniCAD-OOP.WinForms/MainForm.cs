using MiniCAD_OOP.Models;
using MiniCAD_OOP.Services;
using MiniCAD_OOP.WinForms.Tools;

namespace MiniCAD_OOP.WinForms;

internal sealed class MainForm : Form
{
    private readonly DocumentoCAD documento;
    private readonly CanvasControl canvas;
    private readonly ToolStripButton puntoButton;
    private readonly ToolStripButton lineaButton;
    private readonly ToolStripButton selezioneButton;
    private readonly ToolStripButton eliminaButton;
    private readonly ToolStripStatusLabel statoLabel;
    private readonly PointTool strumentoPunto;
    private readonly LineTool strumentoLinea;
    private readonly SelectionTool strumentoSelezione;

    public MainForm()
    {
        Text = "MiniCAD-OOP";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 700);

        documento = CreaDocumentoDimostrativo();

        var contesto = new DrawingToolContext(CreaGeneratoreId(), documento.Entita[0].Layer);
        strumentoPunto = new PointTool(contesto);
        strumentoLinea = new LineTool(contesto);

        statoLabel = new ToolStripStatusLabel("Pronto.");
        canvas = new CanvasControl(documento);
        canvas.StatusChanged += messaggio => statoLabel.Text = messaggio;
        strumentoSelezione = new SelectionTool(
            documento,
            coordinate => canvas.WorldToScreen(coordinate.X, coordinate.Y));

        selezioneButton = new ToolStripButton("Selezione") { CheckOnClick = false };
        eliminaButton = new ToolStripButton("Elimina") { Enabled = false };
        canvas.SelectionAvailabilityChanged += disponibile => eliminaButton.Enabled = disponibile;
        puntoButton = new ToolStripButton("Punto") { CheckOnClick = false };
        lineaButton = new ToolStripButton("Linea") { CheckOnClick = false };

        selezioneButton.Click += (_, _) => AttivaStrumentoSelezione();
        eliminaButton.Click += (_, _) => EliminaEntitaSelezionata();
        puntoButton.Click += (_, _) => AttivaStrumentoPunto();
        lineaButton.Click += (_, _) => AttivaStrumentoLinea();

        var barraStrumenti = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            Dock = DockStyle.Top
        };
        barraStrumenti.Items.AddRange(new ToolStripItem[]
        {
            selezioneButton,
            eliminaButton,
            new ToolStripSeparator(),
            puntoButton,
            lineaButton
        });

        var barraStato = new StatusStrip { Dock = DockStyle.Bottom };
        barraStato.Items.Add(statoLabel);

        Controls.Add(canvas);
        Controls.Add(barraStato);
        Controls.Add(barraStrumenti);

        AttivaStrumentoPunto();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            canvas.AnnullaOperazione();
            return true;
        }

        if (keyData == Keys.Delete)
        {
            EliminaEntitaSelezionata();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void EliminaEntitaSelezionata()
    {
        canvas.EliminaEntitaSelezionata();
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

    private Func<int> CreaGeneratoreId()
    {
        var idUsati = new HashSet<int>();

        foreach (var entita in documento.Entita)
        {
            idUsati.Add(entita.Id);

            switch (entita)
            {
                case Punto punto:
                    idUsati.Add(punto.Id);
                    break;
                case Linea linea:
                    idUsati.Add(linea.Inizio.Id);
                    idUsati.Add(linea.Fine.Id);
                    break;
                case FiguraGeometrica figura:
                    foreach (var vertice in figura.Vertici)
                    {
                        idUsati.Add(vertice.Id);
                    }

                    break;
            }
        }

        var prossimoId = 0;
        return () =>
        {
            while (idUsati.Contains(prossimoId))
            {
                if (prossimoId == int.MaxValue)
                {
                    throw new InvalidOperationException("Non sono disponibili altri ID.");
                }

                prossimoId++;
            }

            var id = prossimoId;
            idUsati.Add(id);
            if (prossimoId < int.MaxValue)
            {
                prossimoId++;
            }

            return id;
        };
    }

    private void AttivaStrumentoPunto()
    {
        canvas.UsaStrumento(strumentoPunto);
        selezioneButton.Checked = false;
        puntoButton.Checked = true;
        lineaButton.Checked = false;
    }

    private void AttivaStrumentoLinea()
    {
        canvas.UsaStrumento(strumentoLinea);
        selezioneButton.Checked = false;
        puntoButton.Checked = false;
        lineaButton.Checked = true;
    }

    private void AttivaStrumentoSelezione()
    {
        canvas.UsaStrumento(strumentoSelezione);
        selezioneButton.Checked = true;
        puntoButton.Checked = false;
        lineaButton.Checked = false;
    }
}
