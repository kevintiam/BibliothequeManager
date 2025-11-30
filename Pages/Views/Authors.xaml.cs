using BibliothequeManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Pages.Views;

public partial class Authors : ContentPage
{
	public Authors()
	{
		InitializeComponent();

        SearchButton.Clicked += OnSearchClicked;
        SearchEntry.TextChanged += OnSearchTextChanged;
        AuthorsCollectionView.SelectionChanged += OnAuthorSelectionChanged;

        ChargerAuteurs();
        
	}

    private void OnFloatingAddClicked(object sender, EventArgs e)
    {
        FormulaireLivres.IsVisible = !FormulaireLivres.IsVisible;

        if(FormulaireLivres.IsVisible && AuthorsCollectionView.SelectedItems == null)
        {
            ReinitialiserFormulaire();
            FormTitle.Text = "Ajouter un auteur"; 
            ModifierButton.IsVisible = false;
            AjouterButton.IsVisible = true;
        }
    }

    private void ReinitialiserFormulaire()
    {
        PrenomEntry.Text = "";
        NomEntry.Text = "";
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

    private async void OnAuthorSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedAuthor = e.CurrentSelection.FirstOrDefault();
        if (selectedAuthor != null)
        {
            dynamic auteur = selectedAuthor;
            int authorId = auteur.Id;
            using var Donnees = new BibliothequeContext();
            var auteurDetails = await Donnees.Auteurs
                .FirstOrDefaultAsync(a => a.Id == authorId);
            if (auteurDetails != null)
            {
                PrenomEntry.Text = auteurDetails.Prenom;
                NomEntry.Text = auteurDetails.Nom;
                FormTitle.Text = "Modifier un auteur";
                ModifierButton.IsVisible = true;
                AjouterButton.IsVisible = false;
                FormulaireLivres.IsVisible = true;
            }
        }
    }

    // filtrage des auteurs
    private void FiltrerAuteurs(string searchText)
    {
        using var Donnees = new BibliothequeContext();
        try
        {
            var auteurRechercher = Donnees.Auteurs
                .Include(a => a.Livres)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                auteurRechercher = auteurRechercher.Where(a =>
                    a.Nom.Contains(searchText) ||
                    a.Prenom.Contains(searchText));
            }

            var auteurs = auteurRechercher
                .Select(a => new
                {
                    a.Id,
                    a.Nom,
                    a.Prenom,
                    NombreLivres = a.Livres.Count
                })
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .AsNoTracking()
                .ToList();

            AuthorsCollectionView.ItemsSource = auteurs;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur filtrage: {ex.Message}");
        }
    }

    private void OnSearchClicked(object? sender, EventArgs e)
    {
        FiltrerAuteurs(SearchEntry.Text);
    }

    // recherche en temps reel
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length >= 3)
        {
            FiltrerAuteurs(e.NewTextValue);
        }
    }
}