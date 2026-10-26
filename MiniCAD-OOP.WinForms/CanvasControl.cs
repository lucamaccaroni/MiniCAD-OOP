using MiniCAD_OOP.Models;
using MiniCAD_OOP.Services;
using MiniCAD_OOP.WinForms.Tools;
using DrawingColor = System.Drawing.Color;
using DrawingPointF = System.Drawing.PointF;

namespace MiniCAD_OOP.WinForms;

public sealed class CanvasControl : Control
{
    private const float MargineVista = 40;
    private const int SpaziaturaGriglia = 1;
    private readonly DocumentoCAD documento;
    private IDrawingTool? strumentoAttivo;
    private EntitaGeometrica? entitaSelezionata;

    public CanvasControl(DocumentoCAD documento)
    {
        ArgumentNullException.ThrowIfNull(documento);
        this.documento = documento;

        BackColor = DrawingColor.White;
        DoubleBuffered = true;
        ResizeRedraw = true;
        TabStop = true;
        Dock = DockStyle.Fill;
    }

    public event Action<string>? StatusChanged;

    public event Action<bool>? SelectionAvailabilityChanged;

    public bool HaSelezione => entitaSelezionata is not null;

    public bool EliminaEntitaSelezionata()
    {
        if (entitaSelezionata is null)
        {
            return false;
        }

        var entita = entitaSelezionata;
        var eliminata = documento.EliminaEntita(entita.Id);
        entitaSelezionata = null;
        SelectionAvailabilityChanged?.Invoke(false);
        Invalidate();

        SegnalaStato(eliminata
            ? $"Eliminata {entita.Nome}."
            : $"{entita.Nome} non è più presente nel documento.");
        return eliminata;
    }

    public DrawingPointF WorldToScreen(double x, double y)
    {
        var scala = CalcolaScala();
        var centroX = ClientSize.Width / 2f;
        var centroY = ClientSize.Height / 2f;

        return new DrawingPointF(
            centroX + (float)(x * scala),
            centroY - (float)(y * scala));
    }

    public CoordinateMondo ScreenToWorld(Point posizione)
    {
        var scala = CalcolaScala();
        var centroX = ClientSize.Width / 2d;
        var centroY = ClientSize.Height / 2d;

        return new CoordinateMondo(
            (posizione.X - centroX) / scala,
            (centroY - posizione.Y) / scala);
    }

    public void UsaStrumento(IDrawingTool strumento)
    {
        ArgumentNullException.ThrowIfNull(strumento);
        if (ReferenceEquals(strumentoAttivo, strumento))
        {
            return;
        }

        if (strumentoAttivo is not null)
        {
            ScollegaEventiStrumento(strumentoAttivo);
            strumentoAttivo.Cancel();
        }

        strumentoAttivo = strumento;
        CollegaEventiStrumento(strumentoAttivo);
        SegnalaStato("Strumento attivo.");
        Invalidate();
    }

    public void AnnullaOperazione()
    {
        strumentoAttivo?.Cancel();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();

        if (e.Button == MouseButtons.Left)
        {
            strumentoAttivo?.OnMouseDown(CreaPosizionePuntatore(e.Location));
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        strumentoAttivo?.OnMouseMove(CreaPosizionePuntatore(e.Location));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(DrawingColor.White);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        DisegnaGriglia(e.Graphics);
        DisegnaAssi(e.Graphics);

        foreach (var entita in documento.Entita)
        {
            DisegnaEntita(e.Graphics, entita);
        }

        if (entitaSelezionata is not null)
        {
            DisegnaEvidenziazione(e.Graphics, entitaSelezionata);
        }

        strumentoAttivo?.DrawPreview(e.Graphics, coordinate => WorldToScreen(coordinate.X, coordinate.Y));
    }

    private PosizionePuntatore CreaPosizionePuntatore(Point posizioneSchermo)
    {
        return new PosizionePuntatore(posizioneSchermo, ScreenToWorld(posizioneSchermo));
    }

    private void SelezionaEntita(EntitaGeometrica? entita)
    {
        var avevaSelezione = entitaSelezionata is not null;
        entitaSelezionata = entita;
        Invalidate();

        var haSelezione = entitaSelezionata is not null;
        if (avevaSelezione != haSelezione)
        {
            SelectionAvailabilityChanged?.Invoke(haSelezione);
        }
    }

    private void CollegaEventiStrumento(IDrawingTool strumento)
    {
        strumento.StatusChanged += SegnalaStato;

        if (strumento is IEntityCreatingTool strumentoCreazione)
        {
            strumentoCreazione.EntityCreated += AggiungiEntita;
        }

        if (strumento is IPreviewDrawingTool strumentoAnteprima)
        {
            strumentoAnteprima.PreviewChanged += Invalidate;
        }

        if (strumento is ISelectionTool strumentoSelezione)
        {
            strumentoSelezione.SelectionChanged += SelezionaEntita;
        }
    }

    private void ScollegaEventiStrumento(IDrawingTool strumento)
    {
        strumento.StatusChanged -= SegnalaStato;

        if (strumento is IEntityCreatingTool strumentoCreazione)
        {
            strumentoCreazione.EntityCreated -= AggiungiEntita;
        }

        if (strumento is IPreviewDrawingTool strumentoAnteprima)
        {
            strumentoAnteprima.PreviewChanged -= Invalidate;
        }

        if (strumento is ISelectionTool strumentoSelezione)
        {
            strumentoSelezione.SelectionChanged -= SelezionaEntita;
        }
    }

    private float CalcolaScala()
    {
        var larghezzaDisponibile = Math.Max(1, ClientSize.Width - 2 * MargineVista);
        var altezzaDisponibile = Math.Max(1, ClientSize.Height - 2 * MargineVista);
        return Math.Max(1, Math.Min(larghezzaDisponibile, altezzaDisponibile) / 24f);
    }

    private void DisegnaGriglia(Graphics graphics)
    {
        using var penna = new Pen(DrawingColor.FromArgb(238, 238, 238));
        var scala = CalcolaScala();
        var mezzoLarghezza = ClientSize.Width / (2 * scala);
        var mezzoAltezza = ClientSize.Height / (2 * scala);
        var minimoX = (int)Math.Floor(-mezzoLarghezza);
        var massimoX = (int)Math.Ceiling(mezzoLarghezza);
        var minimoY = (int)Math.Floor(-mezzoAltezza);
        var massimoY = (int)Math.Ceiling(mezzoAltezza);

        for (var x = minimoX; x <= massimoX; x += SpaziaturaGriglia)
        {
            var inizio = WorldToScreen(x, minimoY * SpaziaturaGriglia);
            var fine = WorldToScreen(x, massimoY * SpaziaturaGriglia);
            graphics.DrawLine(penna, inizio, fine);
        }

        for (var y = minimoY; y <= massimoY; y += SpaziaturaGriglia)
        {
            var inizio = WorldToScreen(minimoX * SpaziaturaGriglia, y);
            var fine = WorldToScreen(massimoX * SpaziaturaGriglia, y);
            graphics.DrawLine(penna, inizio, fine);
        }
    }

    private void DisegnaAssi(Graphics graphics)
    {
        using var penna = new Pen(DrawingColor.FromArgb(150, 150, 150), 1.5f);
        var origine = WorldToScreen(0, 0);

        graphics.DrawLine(penna, 0, origine.Y, ClientSize.Width, origine.Y);
        graphics.DrawLine(penna, origine.X, 0, origine.X, ClientSize.Height);
    }

    private void DisegnaEntita(Graphics graphics, EntitaGeometrica entita)
    {
        var colore = ConvertiColore(entita.Colore);
        using var penna = new Pen(colore, 2);
        using var pennello = new SolidBrush(colore);

        switch (entita)
        {
            case Punto punto:
                DisegnaPunto(graphics, punto, pennello);
                break;
            case Linea linea:
                graphics.DrawLine(
                    penna,
                    ToPointF(linea.Inizio.X, linea.Inizio.Y),
                    ToPointF(linea.Fine.X, linea.Fine.Y));
                break;
            case FiguraGeometrica figura:
                var vertici = figura.Vertici
                    .Select(punto => ToPointF(punto.X, punto.Y))
                    .ToArray();
                graphics.DrawPolygon(penna, vertici);
                break;
        }
    }

    private void DisegnaPunto(Graphics graphics, Punto punto, Brush pennello)
    {
        const float raggio = 4;
        var centro = ToPointF(punto.X, punto.Y);
        graphics.FillEllipse(
            pennello,
            centro.X - raggio,
            centro.Y - raggio,
            raggio * 2,
            raggio * 2);
    }

    private void DisegnaEvidenziazione(Graphics graphics, EntitaGeometrica entita)
    {
        using var penna = new Pen(DrawingColor.DodgerBlue, 3);

        switch (entita)
        {
            case Punto punto:
                var centro = ToPointF(punto.X, punto.Y);
                graphics.DrawEllipse(penna, centro.X - 7, centro.Y - 7, 14, 14);
                break;
            case Linea linea:
                graphics.DrawLine(
                    penna,
                    ToPointF(linea.Inizio.X, linea.Inizio.Y),
                    ToPointF(linea.Fine.X, linea.Fine.Y));
                break;
            case FiguraGeometrica figura:
                var vertici = figura.Vertici
                    .Select(vertice => ToPointF(vertice.X, vertice.Y))
                    .ToArray();
                graphics.DrawPolygon(penna, vertici);
                break;
        }
    }

    private void AggiungiEntita(EntitaGeometrica entita)
    {
        documento.AggiungiEntita(entita);
        Invalidate();
        SegnalaStato($"Aggiunta {entita.Nome}. Entità nel documento: {documento.ContaEntita()}.");
    }

    private void SegnalaStato(string messaggio)
    {
        StatusChanged?.Invoke(messaggio);
    }

    private DrawingPointF ToPointF(double x, double y)
    {
        return WorldToScreen(x, y);
    }

    private static DrawingColor ConvertiColore(Colore colore)
    {
        return colore switch
        {
            Colore.Nero => DrawingColor.Black,
            Colore.Rosso => DrawingColor.Red,
            Colore.Verde => DrawingColor.Green,
            Colore.Blu => DrawingColor.Blue,
            Colore.Giallo => DrawingColor.Gold,
            Colore.Magenta => DrawingColor.Magenta,
            Colore.Ciano => DrawingColor.Cyan,
            _ => throw new ArgumentOutOfRangeException(nameof(colore), "Il colore non è supportato.")
        };
    }
}
