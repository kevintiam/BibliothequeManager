using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using BibliothequeManager.Services;
using BibliothequeManager.Views;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Pages.Views;

public partial class Books : ContentPage
{
    private readonly SessionUser session;

    /// <summary>
    /// Constructeur de la page de gestion des livres
    /// </summary>
    /// <param name="user">Session utilisateur actuelle</param>
    public Books(SessionUser user)
    {
        InitializeComponent();
        session = user;

        // Vérifier si l'utilisateur est connecté
        if (!session.EstConnecte)
        {
            Application.Current.MainPage = new NavigationPage(new Connexion());
            return;
        }

        // Attacher les gestionnaires d'événements
        SearchButton.Clicked += OnSearchClicked;
        SearchEntry.TextChanged += OnSearchTextChanged;
        BooksCollectionView.SelectionChanged += OnBookSelectionChanged;

        // Charger les données initiales
        ChargerLivres();
        ChargerAuteurs();
        ChargerCategories();
    }

    /// <summary>
    /// Gère le clic sur le bouton flottant d'ajout/modification
    /// </summary>
    private void OnFloatingAddClicked(object sender, EventArgs e)
    {
        // Basculer la visibilité du formulaire
        if (FormulaireLivres.IsVisible)
        {
            FormulaireLivres.IsVisible = false;
        }
        else
        {
            // Réinitialiser et configurer le formulaire pour l'ajout
            ReinitialiserFormulaire();
            FormTitle.Text = App.Localized["AddBook"];
            ModifierButton.IsVisible = false;
            AjouterButton.IsVisible = true;
            SupprimerButton.IsVisible = false;
            FormulaireLivres.IsVisible = true;
        }
    }

    /// <summary>
    /// Réinitialise les champs du formulaire à leurs valeurs par défaut
    /// </summary>
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

    /// <summary>
    /// Charge la liste des livres depuis la base de données
    /// </summary>
    private void ChargerLivres()
    {
        using var context = new BibliothequeContext();
        try
        {
            // Récupérer les livres avec leurs relations
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

    /// <summary>
    /// Charge la liste des auteurs pour le formulaire
    /// </summary>
    private void ChargerAuteurs()
    {
        using var context = new BibliothequeContext();
        try
        {
            // Récupérer tous les auteurs
            var auteurs = context.Auteurs
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .AsNoTracking()
                .ToList();

            PickerAuthorSelect.ItemsSource = auteurs;

            PickerAuthorSelect.ItemDisplayBinding = new Binding("NomComplet");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
        }
    }

    /// <summary>
    /// Charge la liste des catégories pour le formulaire
    /// </summary>
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

    /// <summary>
    /// Gère la sélection d'un livre dans la liste
    /// </summary>
    private async void OnBookSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Récupérer le livre sélectionné
        var selectedBook = e.CurrentSelection.FirstOrDefault() as Livres;

        if (selectedBook != null)
        {
            int bookId = selectedBook.Id;

            // Charger les détails complets du livre
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
                DatePickerPublication.Date = livreDetails.AnneePublication;

                // Sélectionner l'auteur dans le Picker
                var auteurs = PickerAuthorSelect.ItemsSource as List<Auteur>;
                if (auteurs != null)
                {
                    var auteurIndex = auteurs.FindIndex(a => a.Id == livreDetails.AuteurId);
                    PickerAuthorSelect.SelectedIndex = auteurIndex;
                }

                // Sélectionner la catégorie dans le Picker
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

    /// <summary>
    /// Filtre les livres en fonction du texte de recherche
    /// </summary>
    /// <param name="searchText">Texte à rechercher</param>
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

            // Appliquer le filtre si un texte est fourni
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

    /// <summary>
    /// Gère le clic sur le bouton de recherche
    /// </summary>
    private void OnSearchClicked(object? sender, EventArgs e)
    {
        FiltrerLivres(SearchEntry.Text);
    }

    /// <summary>
    /// Gère la recherche en temps réel lors de la saisie
    /// </summary>
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length >= 2)
        {
            FiltrerLivres(e.NewTextValue);
        }
    }

    /// <summary>
    /// Gère la modification d'un livre existant
    /// </summary>
    private async void OnModifierClicked(object sender, EventArgs e)
    {
        // Récupérer le livre sélectionné
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

                // Mise à jour des champs principaux du livre
                livreToUpdate.Titre = TxtTitre.Text;
                livreToUpdate.ISBN = TxtISBN.Text;
                livreToUpdate.AnneePublication = DatePickerPublication.Date;

                if (auteur != null)
                    livreToUpdate.AuteurId = auteur.Id;

                var categorieExistante = livreToUpdate.LivreCategories.FirstOrDefault();
                if (categorieExistante != null)
                    context.LivreCategories.Remove(categorieExistante);

                if (categorie != null)
                {
                    var nouvelleLivreCategorie = new LivreCategorie
                    {
                        LivreId = livreToUpdate.Id,
                        CategorieId = categorie.Id
                    };
                    context.LivreCategories.Add(nouvelleLivreCategorie);
                }

                // Valider et mettre à jour le nombre de pages
                if (!int.TryParse(TxtPages.Text, out var nombrePages))
                {
                    await ErrorPopup.Show("Nombre de pages invalide.", this);
                    return;
                }

                foreach (var exemplaire in livreToUpdate.Exemplaires)
                {
                    exemplaire.NombrePage = nombrePages;
                }

                // Gérer le nombre d'exemplaires
                if (!int.TryParse(TxtExemplaires.Text, out var nombreExemplairesSouhaite) || nombreExemplairesSouhaite < 0)
                {
                    await ErrorPopup.Show("Nombre d'exemplaires invalide.", this);
                    return;
                }

                var nombreActuel = livreToUpdate.Exemplaires.Count;

                if (nombreExemplairesSouhaite > nombreActuel)
                {
                    // Ajouter les nouveaux exemplaires
                    for (int i = nombreActuel + 1; i <= nombreExemplairesSouhaite; i++)
                    {
                        var exemplaire = new Exemplaire
                        {
                            LivreId = livreToUpdate.Id,
                            NombrePage = nombrePages,
                            CodeBarre = $"{livreToUpdate.ISBN}-{i:D5}",
                            EstDisponible = true,
                            Etat = "Neuf"
                        };
                        context.Exemplaires.Add(exemplaire);
                    }
                }
                else if (nombreExemplairesSouhaite < nombreActuel)
                {
                    // Supprimer les exemplaires en surplus (les plus récents)
                    var aSupprimer = livreToUpdate.Exemplaires
                        .OrderByDescending(e => e.CodeBarre)
                        .Take(nombreActuel - nombreExemplairesSouhaite)
                        .ToList();

                    context.Exemplaires.RemoveRange(aSupprimer);
                }

                await context.SaveChangesAsync();
                await SuccessPopup.Show("Livre modifié avec succès !", this);

                ChargerLivres();
                ReinitialiserFormulaire();
                FormulaireLivres.IsVisible = false;
            }
        }
    }

    /// <summary>
    /// Gère l'ajout d'un nouveau livre
    /// </summary>
    private async void OnAjouterClicked(object sender, EventArgs e)
    {
        using var context = new BibliothequeContext();
        var auteur = PickerAuthorSelect.SelectedItem as Auteur;
        var categorie = PickerCathegorieSelect.SelectedItem as Categorie;
        var anneePublication = DatePickerPublication.Date;

        // Validation des données obligatoires
        if (auteur == null)
        {
            await ErrorPopup.Show("Veuillez sélectionner un auteur", this);
            return;
        }

        // Validation des champs numériques
        if (!int.TryParse(TxtExemplaires.Text, out int nombreExemplaires) || nombreExemplaires < 1)
        {
            await ErrorPopup.Show("Nombre d'exemplaires invalide", this);
            return;
        }

        if (!int.TryParse(TxtPages.Text, out int nombrePages) || nombrePages < 1)
        {
            await ErrorPopup.Show("Nombre de pages invalide", this);
            return;
        }

        // Créer le livre principal
        var nouveauLivre = new Livres
        {
            Titre = TxtTitre.Text,
            ISBN = TxtISBN.Text,
            AuteurId = auteur.Id,
            AnneePublication = anneePublication
        };

        context.Livres.Add(nouveauLivre);
        await context.SaveChangesAsync(); // Sauvegarder pour obtenir l'ID

        // Ajouter la catégorie si sélectionnée
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

        // Sauvegarder les exemplaires et la catégorie
        await context.SaveChangesAsync();

        // Afficher un message de succès
        await SuccessPopup.Show("Livre ajouté avec succès !", this);

        // Rafraîchir l'affichage
        ChargerLivres();
        ReinitialiserFormulaire();
        FormulaireLivres.IsVisible = false;
    }

    /// <summary>
    /// Gère la suppression d'un livre
    /// </summary>
    private async void OnSupprimerClicked(object sender, EventArgs e)
    {
        // Récupérer le livre sélectionné
        var selectedBook = BooksCollectionView.SelectedItem as Livres;

        if (selectedBook != null)
        {
            // Créer et afficher un popup de confirmation
            var popup = new ConfirmationPopup
            {
                Title = App.Localized["ConfirmDelete"],
                Message = App.Localized["ReallydeleteBook"]
            };

            // Gérer la réponse de l'utilisateur
            popup.OnCompleted += async (confirmed) =>
            {
                if (confirmed)
                {
                    await SupprimerLivre(selectedBook.Id);
                    await SuccessPopup.Show(App.Localized["BookDeleted"], this);
                }
            };

            // Afficher le popup
            await Navigation.PushModalAsync(popup);
        }
    }

    /// <summary>
    /// Supprime un livre de la base de données
    /// </summary>
    /// <param name="id">Identifiant du livre à supprimer</param>
    private async Task SupprimerLivre(int id)
    {
        using var context = new BibliothequeContext();

        // Charger le livre avec toutes ses relations
        var livre = await context.Livres
            .Include(l => l.Exemplaires)
            .Include(l => l.LivreCategories)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (livre != null)
        {
            // Supprimer d'abord les relations (catégories, exemplaires)
            context.LivreCategories.RemoveRange(livre.LivreCategories);
            context.Exemplaires.RemoveRange(livre.Exemplaires);

            // Puis supprimer le livre lui-même
            context.Livres.Remove(livre);

            // Sauvegarder les modifications
            await context.SaveChangesAsync();

            // Rafraîchir l'affichage
            ChargerLivres();
            ReinitialiserFormulaire();

            // Masquer le formulaire et réinitialiser la sélection
            FormulaireLivres.IsVisible = false;
            BooksCollectionView.SelectedItem = null;
        }
    }
}