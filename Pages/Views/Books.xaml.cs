using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Pages.Views;

public partial class Books : ContentPage
{
    public Books()
    {
        InitializeComponent();

        SearchButton.Clicked += OnSearchClicked;
        SearchEntry.TextChanged += OnSearchTextChanged;
        BooksCollectionView.SelectionChanged += OnBookSelectionChanged;

        ChargerLivres();
        ChargerAuteurs();
        ChargerCategories();
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
            FormTitle.Text = App.Localized["AddBook"] ;
            ModifierButton.IsVisible = false;
            AjouterButton.IsVisible = true;
            SupprimerButton.IsVisible = false;
            FormulaireLivres.IsVisible = true;
        }
    }

    private void ReinitialiserFormulaire()
    {
        TxtTitre.Text = "";
        TxtISBN.Text = "";
        TxtPages.Text = "";
        TxtExemplaires.Text = "";
        PickerAuthorSelect.SelectedIndex = -1;
        PickerCathegorieSelect.SelectedIndex = -1;
        DatePickerPublication.Date = DateTime.Now;
    }

    private void ChargerLivres()
    {
        using var context = new BibliothequeContext();
        try
        {
            var livres = context.Livres
                .Include(l => l.Auteur)
                .Include(l => l.LivreCategories)
                    .ThenInclude(lc => lc.Categorie)
                .Include(l => l.Exemplaires)
                .OrderBy(l => l.Titre)
                .AsNoTracking()
                .ToList();

            BooksCollectionView.ItemsSource = livres;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
        }
    }

    private void ChargerAuteurs()
    {
        using var context = new BibliothequeContext();
        try
        {
            var auteurs = context.Auteurs
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .AsNoTracking()
                .ToList();

            PickerAuthorSelect.ItemsSource = auteurs;

            // ✅ UTILISER LA PROPRIÉTÉ NomComplet
            PickerAuthorSelect.ItemDisplayBinding = new Binding("NomComplet");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
        }
    }


    private void ChargerCategories()
    {
        using var context = new BibliothequeContext();
        try
        {
            var categories = context.Categories
                .OrderBy(c => c.Nom)
                .AsNoTracking()
                .ToList();

            PickerCathegorieSelect.ItemsSource = categories;
            PickerCathegorieSelect.ItemDisplayBinding = new Binding("Nom");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
        }
    }

    private async void OnBookSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedBook = e.CurrentSelection.FirstOrDefault() as Livres;
        if (selectedBook != null)
        {
            int bookId = selectedBook.Id;
            using var context = new BibliothequeContext();
            var livreDetails = await context.Livres
                .Include(l => l.Auteur)
                .Include(l => l.LivreCategories)
                    .ThenInclude(lc => lc.Categorie)
                .Include(l => l.Exemplaires)
                .FirstOrDefaultAsync(l => l.Id == bookId);

            if (livreDetails != null)
            {
                TxtTitre.Text = livreDetails.Titre;
                TxtISBN.Text = livreDetails.ISBN;

                // Sélectionner l'auteur
                var auteurs = PickerAuthorSelect.ItemsSource as List<Auteur>;
                if (auteurs != null)
                {
                    var auteurIndex = auteurs.FindIndex(a => a.Id == livreDetails.AuteurId);
                    PickerAuthorSelect.SelectedIndex = auteurIndex;
                }

                // Sélectionner la catégorie
                var categorie = livreDetails.LivreCategories.FirstOrDefault()?.Categorie;
                if (categorie != null)
                {
                    var categories = PickerCathegorieSelect.ItemsSource as List<Categorie>;
                    if (categories != null)
                    {
                        var categorieIndex = categories.FindIndex(c => c.Id == categorie.Id);
                        PickerCathegorieSelect.SelectedIndex = categorieIndex;
                    }
                }

                // Remplir les exemplaires
                var premierExemplaire = livreDetails.Exemplaires.FirstOrDefault();
                if (premierExemplaire != null)
                {
                    TxtPages.Text = premierExemplaire.NombrePage.ToString();
                }
                TxtExemplaires.Text = livreDetails.Exemplaires.Count.ToString();

                FormTitle.Text = App.Localized["Edit/Delete"];
                ModifierButton.IsVisible = true;
                AjouterButton.IsVisible = false;
                SupprimerButton.IsVisible = true;
                FormulaireLivres.IsVisible = true;
            }
        }
    }

    // Filtrage des livres
    private async void FiltrerLivres(string searchText)
    {
        using var context = new BibliothequeContext();
        try
        {
            var query = context.Livres
                .Include(l => l.Auteur)
                .Include(l => l.LivreCategories)
                    .ThenInclude(lc => lc.Categorie)
                .Include(l => l.Exemplaires)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(l =>
                    l.Titre.Contains(searchText) ||
                    l.ISBN.Contains(searchText) ||
                    (l.Auteur != null && l.Auteur.Nom.Contains(searchText)) ||
                    (l.Auteur != null && l.Auteur.Prenom.Contains(searchText)));
            }

            var livres = await query
                .OrderBy(l => l.Titre)
                .AsNoTracking()
                .ToListAsync();

            BooksCollectionView.ItemsSource = livres;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de filtrer : {ex.Message}", "OK");
        }
    }

    private void OnSearchClicked(object? sender, EventArgs e)
    {
        FiltrerLivres(SearchEntry.Text);
    }

    // Recherche en temps réel
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length >= 3)
        {
            FiltrerLivres(e.NewTextValue);
        }
    }

    // Bouton Modifier
    private async void OnModifierClicked(object sender, EventArgs e)
    {
        var selectedBook = BooksCollectionView.SelectedItem as Livres;
        if (selectedBook != null)
        {
            int bookId = selectedBook.Id;
            using var context = new BibliothequeContext();
            var livreToUpdate = await context.Livres
                .Include(l => l.LivreCategories)
                .Include(l => l.Exemplaires)
                .FirstOrDefaultAsync(l => l.Id == bookId);

            if (livreToUpdate != null)
            {
                var auteur = PickerAuthorSelect.SelectedItem as Auteur;
                var categorie = PickerCathegorieSelect.SelectedItem as Categorie;

                livreToUpdate.Titre = TxtTitre.Text;
                livreToUpdate.ISBN = TxtISBN.Text;

                if (auteur != null)
                {
                    livreToUpdate.AuteurId = auteur.Id;
                }

                // Mettre à jour la catégorie
                var categorieExistante = livreToUpdate.LivreCategories.FirstOrDefault();
                if (categorieExistante != null)
                {
                    context.LivreCategories.Remove(categorieExistante);
                }

                if (categorie != null)
                {
                    var nouvelleLivreCategorie = new LivreCategorie
                    {
                        LivreId = livreToUpdate.Id,
                        CategorieId = categorie.Id
                    };
                    context.LivreCategories.Add(nouvelleLivreCategorie);
                }

                // Mettre à jour le nombre de pages des exemplaires existants
                int nombrePages = int.Parse(TxtPages.Text);
                foreach (var exemplaire in livreToUpdate.Exemplaires)
                {
                    exemplaire.NombrePage = nombrePages;
                }

                await context.SaveChangesAsync();
                ChargerLivres();
                ReinitialiserFormulaire();
                FormulaireLivres.IsVisible = false;
            }
        }
    }

    // Bouton Ajouter
    private async void OnAjouterClicked(object sender, EventArgs e)
    {
        using var context = new BibliothequeContext();
        var auteur = PickerAuthorSelect.SelectedItem as Auteur;
        var categorie = PickerCathegorieSelect.SelectedItem as Categorie;

        if (auteur == null)
        {
            await DisplayAlert("Erreur", "Veuillez sélectionner un auteur", "OK");
            return;
        }

        // Créer le livre
        var nouveauLivre = new Livres
        {
            Titre = TxtTitre.Text,
            ISBN = TxtISBN.Text,
            AuteurId = auteur.Id
        };

        context.Livres.Add(nouveauLivre);
        await context.SaveChangesAsync();

        // Ajouter la catégorie
        if (categorie != null)
        {
            var livreCategorie = new LivreCategorie
            {
                LivreId = nouveauLivre.Id,
                CategorieId = categorie.Id
            };
            context.LivreCategories.Add(livreCategorie);
        }

        // Créer les exemplaires
        int nombreExemplaires = int.Parse(TxtExemplaires.Text);
        int nombrePages = int.Parse(TxtPages.Text);

        for (int i = 1; i <= nombreExemplaires; i++)
        {
            var exemplaire = new Exemplaire
            {
                LivreId = nouveauLivre.Id,
                NombrePage = nombrePages,
                CodeBarre = $"{nouveauLivre.ISBN}-{i:D5}",
                EstDisponible = true,
                Etat = "Neuf"
            };
            context.Exemplaires.Add(exemplaire);
        }

        await context.SaveChangesAsync();

        await SuccessPopup.Show("Livre ajouté avec succès !", this);

        ChargerLivres();
        ReinitialiserFormulaire();
        FormulaireLivres.IsVisible = false;
    }

    // Bouton Supprimer
    private async void OnSupprimerClicked(object sender, EventArgs e)
    {
        var selectedBook = BooksCollectionView.SelectedItem as Livres;

        if (selectedBook != null)
        {
            var popup = new ConfirmationPopup
            {
                Title = App.Localized["ConfirmDelete"],
                Message = App.Localized["ReallydeleteBook"]
            };

            popup.OnCompleted += async (confirmed) =>
            {
                if (confirmed)
                {
                    await SupprimerLivre(selectedBook.Id);
                }
            };

            await Navigation.PushModalAsync(popup);
        }
    }

    private async Task SupprimerLivre(int id)
    {
        using var context = new BibliothequeContext();
        var livre = await context.Livres
            .Include(l => l.Exemplaires)
            .Include(l => l.LivreCategories)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (livre != null)
        {
            // Supprimer les catégories associées
            context.LivreCategories.RemoveRange(livre.LivreCategories);

            // Supprimer les exemplaires
            context.Exemplaires.RemoveRange(livre.Exemplaires);

            // Supprimer le livre
            context.Livres.Remove(livre);

            await context.SaveChangesAsync();
            ChargerLivres();
            ReinitialiserFormulaire();
            FormulaireLivres.IsVisible = false;
            BooksCollectionView.SelectedItem = null;
        }
    }
}