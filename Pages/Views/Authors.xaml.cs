using BibliothequeManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Pages.Views;

public partial class Authors : ContentPage
{
	public Authors()
	{
		InitializeComponent();

        ChargerAuteurs();
        
	}

    private void OnFloatingAddClicked(object sender, EventArgs e)
    {
        FormulaireLivres.IsVisible = !FormulaireLivres.IsVisible;
    }

    private void ChargerAuteurs()
    {
        using var Donnees = new BibliothequeContext();
        try
        {
            var auteurs = Donnees.Auteurs
                .Include(a => a.Livres)
                .Select(a => new
                {
                    a.Id,
                    a.Nom,
                    a.Prenom,
                    NombreLivres = a.Livres.Count,
                })
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .AsNoTracking()
                .ToList();

            AuthorsCollectionView.ItemsSource = auteurs;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur chargement auteurs: {ex.Message}");
        }
    }

    private string GetInitiales(string prenom, string nom)
    {
        if (string.IsNullOrWhiteSpace(prenom) || string.IsNullOrWhiteSpace(nom))
            return "??";

        return $"{prenom[0]}{nom[0]}".ToUpper();
    }
}