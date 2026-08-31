namespace MiniCAD_OOP.Models;

public class Layer
{
    private string nome;
    private bool visibile;

    public string Nome
    {
        get { return nome; }
    }

    public bool Visibile
    {
        get { return visibile; }
    }

    public Layer(string nome)
    {
        this.nome = nome;
        visibile = true;
    }

    public void Attiva()
    {
        visibile = true;
    }

    public void Disattiva()
    {
        visibile = false;
    }
}