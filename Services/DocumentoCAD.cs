using MiniCAD_OOP.Models;
using System.Collections.ObjectModel;

namespace MiniCAD_OOP.Services;

public class DocumentoCAD
{
    private readonly List<EntitaGeometrica> entita = new();
    private readonly ReadOnlyCollection<EntitaGeometrica> vistaEntita;

    public DocumentoCAD(string nome)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        Nome = nome;
        vistaEntita = entita.AsReadOnly();
    }

    public string Nome { get; }

    public IReadOnlyList<EntitaGeometrica> Entita => vistaEntita;

    public void AggiungiEntita(EntitaGeometrica entita)
    {
        ArgumentNullException.ThrowIfNull(entita);

        if (this.entita.Any(esistente => esistente.Id == entita.Id))
        {
            throw new InvalidOperationException($"Esiste già un'entità con ID {entita.Id}.");
        }

        this.entita.Add(entita);
    }

    public bool EliminaEntita(int id)
    {
        var indice = entita.FindIndex(entita => entita.Id == id);
        if (indice < 0)
        {
            return false;
        }

        entita.RemoveAt(indice);
        return true;
    }

    public EntitaGeometrica CercaEntita(int id)
    {
        return entita.Find(entita => entita.Id == id)
            ?? throw new KeyNotFoundException($"Non esiste un'entità con ID {id}.");
    }

    public void StampaElenco()
    {
        foreach (var entita in this.entita)
        {
            Console.WriteLine(entita.Descrizione());
        }
    }

    public int ContaEntita()
    {
        return entita.Count;
    }
}
