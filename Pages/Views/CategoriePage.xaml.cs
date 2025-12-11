
using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using BibliothequeManager.Services;
using BibliothequeManager.Views;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Pages.Views;

public partial class CategoriePage : ContentPage
{
    // Session utilisateur pour gérer l'authentification
    private readonly SessionUser session;

    /// <summary>
    /// Constructeur de la page de gestion des catégories
    /// </summary>
    /// <param name="user">Session utilisateur actuelle</param>
    public CategoriePage(SessionUser user)
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
        CategoriesCollectionView.SelectionChanged += OnCategorieSelectionChanged;

        // Charger la liste initiale des catégories
        ChargerCategories();
    }

    /// <summary>
    /// Gère le clic sur le bouton flottant d'ajout/modification
    /// </summary>
    private void OnFloatingAddCategorieClicked(object sender, EventArgs e)
    {
        // Basculer la visibilité du formulaire
        if (FormulaireCategorie.IsVisible)
        {
            FormulaireCategorie.IsVisible = false;
        }
        else
        {
            // Réinitialiser et configurer le formulaire pour l'ajout
            ReinitialiserFormulaire();
            FormTitle.Text = App.Localized["AddCategories"];
            ModifierButton.IsVisible = false;
            AjouterButton.IsVisible = true;
            SupprimerButton.IsVisible = false;
            FormulaireCategorie.IsVisible = true;
        }
    }

    /// <summary>
    /// Réinitialise les champs du formulaire à leurs valeurs par défaut
    /// </summary>
    private void ReinitialiserFormulaire()
    {
        NomCategorieEntry.Text = "";
        DescriptionCategorieEntry.Text = "";
    }

    /// <summary>
    /// Charge la liste des catégories depuis la base de données
    /// </summary>
    private void ChargerCategories()
    {
        using var context = new BibliothequeContext();
        try
        {
            // Récupérer les catégories avec leurs livres associés
            var categories = context.Categories
                .Include(c => c.LivreCategories) // Inclure les relations avec les livres
                .OrderBy(c => c.Nom) // Trier par nom
                .AsNoTracking() // Optimisation : lecture seule
                .ToList();

            // Afficher la liste dans la CollectionView
            CategoriesCollectionView.ItemsSource = categories;
        }
        catch (Exception ex)
        {
            // Journaliser l'erreur
            Console.WriteLine($"Erreur : {ex.Message}");
        }
    }

    /// <summary>
    /// Gère la sélection d'une catégorie dans la liste
    /// </summary>
    private async void OnCategorieSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Récupérer la catégorie sélectionnée
        var selectedCategorie = e.CurrentSelection.FirstOrDefault() as Categorie;

        if (selectedCategorie != null)
        {
            // Charger les détails complets de la catégorie
            using var context = new BibliothequeContext();
            var categorieDetails = await context.Categories
                .Include(c => c.LivreCategories)
                .FirstOrDefaultAsync(c => c.Id == selectedCategorie.Id);

            if (categorieDetails != null)
            {
                // Remplir le formulaire avec les données de la catégorie
                NomCategorieEntry.Text = categorieDetails.Nom;
                DescriptionCategorieEntry.Text = categorieDetails.Description;

                // Configurer l'interface pour la modification
                FormTitle.Text = App.Localized["AddEditCategory"];
                ModifierButton.IsVisible = true;
                AjouterButton.IsVisible = false;
                SupprimerButton.IsVisible = true;
                FormulaireCategorie.IsVisible = true;
            }
        }
    }

    /// <summary>
    /// Filtre les catégories en fonction du texte de recherche
    /// </summary>
    /// <param name="searchText">Texte à rechercher</param>
    private async void FiltrerCategories(string searchText)
    {
        using var context = new BibliothequeContext();
        try
        {
            var query = context.Categories
                .Include(c => c.LivreCategories)
                .AsQueryable();

            // Appliquer le filtre si un texte est fourni
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(c => c.Nom.Contains(searchText));
            }

            // Exécuter la requête et récupérer les résultats
            var categories = await query
                .OrderBy(c => c.Nom)
                .AsNoTracking() // Optimisation pour les lectures
                .ToListAsync();

            // Mettre à jour la liste affichée
            CategoriesCollectionView.ItemsSource = categories;
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
        FiltrerCategories(SearchEntry.Text);
    }

    /// <summary>
    /// Gère la recherche en temps réel lors de la saisie
    /// </summary>
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Déclencher la recherche après 3 caractères ou si le champ est vide
        if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length >= 3)
        {
            FiltrerCategories(e.NewTextValue);
        }
    }

    /// <summary>
    /// Gère la modification d'une catégorie existante
    /// </summary>
    private async void OnModifierCategorieClicked(object sender, EventArgs e)
    {
        // Récupérer la catégorie sélectionnée
        var selectedCategorie = CategoriesCollectionView.SelectedItem as Categorie;

        if (selectedCategorie == null) return;

        // Valider les données du formulaire
        var nom = NomCategorieEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nom))
        {
            await ErrorPopup.Show(App.Localized["popErrorCategori1"], this);
            return;
        }

        using var context = new BibliothequeContext();

        // Vérifier l'unicité du nom (sauf pour la catégorie elle-même)
        var existeDeja = await context.Categories
            .AnyAsync(c => c.Nom == nom && c.Id != selectedCategorie.Id);

        if (existeDeja)
        {
            // Utiliser le popup d'erreur pour la cohérence
            await ErrorPopup.Show("Une autre catégorie porte déjà ce nom.", this);
            return;
        }

        // Récupérer et mettre à jour la catégorie
        var categorie = await context.Categories.FindAsync(selectedCategorie.Id);

        if (categorie != null)
        {
            categorie.Nom = nom;
            categorie.Description = DescriptionCategorieEntry.Text?.Trim() ?? string.Empty;

            await context.SaveChangesAsync();

            // Afficher un message de succès
            await SuccessPopup.Show(App.Localized["succespopCategorie1"], this);
            ChargerCategories();
            ReinitialiserFormulaire();

            // Masquer le formulaire
            FormulaireCategorie.IsVisible = false;
        }
    }

    /// <summary>
    /// Gère l'ajout d'une nouvelle catégorie
    /// </summary>
    private async void OnAjouterCategorieClicked(object sender, EventArgs e)
    {
        // Valider les données du formulaire
        var nom = NomCategorieEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(nom))
        {
            // Utiliser DisplayAlert pour plus de cohérence
            await DisplayAlert("Erreur", "Le nom de la catégorie est obligatoire.", "OK");
            return;
        }

        using var context = new BibliothequeContext();

        // Vérifier si la catégorie existe déjà
        if (await context.Categories.AnyAsync(c => c.Nom == nom))
        {
            await DisplayAlert("Erreur", $"La catégorie '{nom}' existe déjà.", "OK");
            return;
        }

        // Créer la nouvelle catégorie
        var newCategorie = new Categorie
        {
            Nom = nom,
            Description = DescriptionCategorieEntry.Text?.Trim() ?? string.Empty
        };

        // Ajouter et sauvegarder
        context.Categories.Add(newCategorie);
        await context.SaveChangesAsync();

        // Afficher un message de succès
        await SuccessPopup.Show(App.Localized["CategorySuccessfullyAdded"], this);

        // Rafraîchir l'affichage
        ChargerCategories();
        ReinitialiserFormulaire();

        // Masquer le formulaire
        FormulaireCategorie.IsVisible = false;
    }

    /// <summary>
    /// Gère la suppression d'une catégorie
    /// </summary>
    private async void OnSupprimerCategorieClicked(object sender, EventArgs e)
    {
        // Récupérer la catégorie sélectionnée
        var selectedCategorie = CategoriesCollectionView.SelectedItem as Categorie;

        if (selectedCategorie != null)
        {
            // Créer et afficher un popup de confirmation
            var popup = new ConfirmationPopup
            {
                Title = App.Localized["ConfirmDelete"],
                Message = string.Format(App.Localized["PopDelCathegori"],
                    selectedCategorie.Nom)
            };

            // Gérer la réponse de l'utilisateur
            popup.OnCompleted += async (confirmed) =>
            {
                if (confirmed)
                {
                    await SupprimerCategorie(selectedCategorie.Id);
                }
            };

            // Afficher le popup
            await Navigation.PushModalAsync(popup);
        }
    }

    /// <summary>
    /// Supprime une catégorie de la base de données
    /// </summary>
    /// <param name="id">Identifiant de la catégorie à supprimer</param>
    private async Task SupprimerCategorie(int id)
    {
        using var context = new BibliothequeContext();

        try
        {
            // Récupérer la catégorie avec ses relations pour vérifier les dépendances
            var categorie = await context.Categories
                .Include(c => c.LivreCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categorie != null)
            {
                // Vérifier si la catégorie est utilisée par des livres
                if (categorie.LivreCategories.Any())
                {
                    await ErrorPopup.Show(
                        "Impossible de supprimer cette catégorie car elle est associée à des livres.",
                        this);
                    return;
                }

                // Supprimer la catégorie
                context.Categories.Remove(categorie);
                await context.SaveChangesAsync();

                // Afficher un message de succès
                await SuccessPopup.Show(App.Localized["CategorySuccessfullyDeleted!"], this);

                // Rafraîchir l'affichage
                ChargerCategories();
                ReinitialiserFormulaire();

                // Masquer le formulaire et réinitialiser la sélection
                FormulaireCategorie.IsVisible = false;
                CategoriesCollectionView.SelectedItem = null;
            }
        }
        catch (DbUpdateException dbEx)
        {
            // Gérer les erreurs de contrainte de clé étrangère
            await ErrorPopup.Show(
                $"Impossible de supprimer la catégorie : {dbEx.InnerException?.Message ?? dbEx.Message}",
                this);
        }
        catch (Exception ex)
        {
            await ErrorPopup.Show($"Erreur lors de la suppression : {ex.Message}", this);
        }
    }
}