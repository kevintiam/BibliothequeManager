using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
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
        if (FormulaireLivres.IsVisible)
        {
            FormulaireLivres.IsVisible = false;
        }
        else
        {
            ReinitialiserFormulaire();
            FormTitle.Text = App.Localized["AddOthor"];
            ModifierButton.IsVisible = false;
            AjouterButton.IsVisible = true;
            FormulaireLivres.IsVisible = true;
            SupprimerButton.IsVisible = false;
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
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .AsNoTracking()
                .ToList();

            AuthorsCollectionView.ItemsSource = auteurs;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
        }
    }

    private async void OnAuthorSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedAuthor = e.CurrentSelection.FirstOrDefault() as Auteur;
        if (selectedAuthor != null)
        {
            int authorId = selectedAuthor.Id;
            using var Donnees = new BibliothequeContext();
            var auteurDetails = await Donnees.Auteurs
                .FirstOrDefaultAsync(a => a.Id == authorId);
            if (auteurDetails != null)
            {
                PrenomEntry.Text = auteurDetails.Prenom;
                NomEntry.Text = auteurDetails.Nom;
                FormTitle.Text = App.Localized["EditOrDelete"];
                ModifierButton.IsVisible = true;
                AjouterButton.IsVisible = false;
                SupprimerButton.IsVisible = true;
                FormulaireLivres.IsVisible = true;
            }
        }
    }

    // filtrage des auteurs
    private async void FiltrerAuteurs(string searchText)
    {
        using var donnees = new BibliothequeContext();
        try
        {
            var query = donnees.Auteurs
                .Include(a => a.Livres) 
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(a =>
                    a.Nom.Contains(searchText) ||
                    a.Prenom.Contains(searchText));
            }

            var auteurs = await query
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .AsNoTracking()
                .ToListAsync();

            AuthorsCollectionView.ItemsSource = auteurs;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de filtrer : {ex.Message}", "OK");
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

    //Bouton Modifier
    private async void OnModifierClicked(object sender, EventArgs e)
    {
        var selectedAuthor = AuthorsCollectionView.SelectedItem as Auteur;
        if (selectedAuthor != null)
        {
            int authorId = selectedAuthor.Id;

            var prenom = PrenomEntry.Text?.Trim();
            var nom = NomEntry.Text?.Trim();

            if (string.IsNullOrEmpty(prenom) || string.IsNullOrEmpty(nom))
            {
                await ErrorPopup.Show(App.Localized["popErrorAutor1"], this);
                return;
            }

            using var Donnees = new BibliothequeContext();
            var auteurToUpdate = await Donnees.Auteurs.FindAsync(authorId);
            if (auteurToUpdate != null)
            {
                auteurToUpdate.Prenom = prenom;
                auteurToUpdate.Nom = nom;
                await SuccessPopup.Show(App.Localized["AuthorSuccessfullyModified"], this);
                await Donnees.SaveChangesAsync();
                ChargerAuteurs();
                ReinitialiserFormulaire();
                FormulaireLivres.IsVisible = false;
            }
        }
    }

    //Bouton Ajouter
    private async void OnAjouterClicked(object sender, EventArgs e)
    {
        var prenom = PrenomEntry.Text?.Trim();
        var nom = NomEntry.Text?.Trim();

        if (string.IsNullOrEmpty(prenom) || string.IsNullOrEmpty(nom))
        {
           await ErrorPopup.Show(App.Localized["popErrorAutor1"],this);
           return;
        }
        
        using var Donnees = new BibliothequeContext();

        if(await Donnees.Auteurs.AnyAsync(a => a.Prenom == prenom) && (await Donnees.Auteurs.AnyAsync(a =>a.Nom == nom)))
        {
            await ErrorPopup.Show(App.Localized["popErrorAutor2"], this);
            return;
        }
        var newAuteur = new Auteur
        {
            Prenom = prenom,
            Nom = nom
        };
        Donnees.Auteurs.Add(newAuteur);
        await Donnees.SaveChangesAsync();

        await SuccessPopup.Show(App.Localized["AuthorSuccessfullyAdded"], this);

        ChargerAuteurs();
        ReinitialiserFormulaire();
        FormulaireLivres.IsVisible = false;
    }

    //Bouton Supprimer
    private async void OnSupprimerClicked(object sender, EventArgs e)
    {
        var selectedAuthor = AuthorsCollectionView.SelectedItem as Auteur;

        if (selectedAuthor !=null)
        {
            var popup = new ConfirmationPopup
            {
                Title = App.Localized["ConfirmDelete"],
                Message = string.Format(App.Localized["popDelAuthor"], selectedAuthor.Prenom, selectedAuthor.Nom)
            };

            popup.OnCompleted += async (confirmed) =>
            {
                if (confirmed)
                {
                    await SupprimerAuteur(selectedAuthor.Id);
                }
            };

            await Navigation.PushModalAsync(popup);
        }
  
    }

    private async Task SupprimerAuteur(int id)
    {
        using var context = new BibliothequeContext();
        var auteur = await context.Auteurs.FindAsync(id);
        if (auteur != null)
        {
            context.Auteurs.Remove(auteur);
            await context.SaveChangesAsync();
            await SuccessPopup.Show(App.Localized["AuthorSuccessfullyDeleted!"], this);
            ChargerAuteurs();
            ReinitialiserFormulaire();
            FormulaireLivres.IsVisible = false;
            AuthorsCollectionView.SelectedItem = null;
        }
    }

}