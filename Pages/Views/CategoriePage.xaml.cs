using BibliothequeManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Pages.Views;

public partial class CategoriePage : ContentPage
{
    public CategoriePage()
    {
        InitializeComponent();

        SearchButton.Clicked += OnSearchClicked;
        SearchEntry.TextChanged += OnSearchTextChanged;
        CategoriesCollectionView.SelectionChanged += OnCategorieSelectionChanged;

        ChargerCategories();
    }

    private void OnFloatingAddCategorieClicked(object sender, EventArgs e)
    {
        if (FormulaireCategorie.IsVisible)
        {
            FormulaireCategorie.IsVisible = false;
        }
        else
        {
            ReinitialiserFormulaire();
            FormTitle.Text = "Ajouter une catégorie";
            ModifierButton.IsVisible = false;
            AjouterButton.IsVisible = true;
            SupprimerButton.IsVisible = false;
            FormulaireCategorie.IsVisible = true;
        }
    }

    private void ReinitialiserFormulaire()
    {
        NomCategorieEntry.Text = "";
        DescriptionCategorieEntry.Text = "";
    }

    private void ChargerCategories()
    {
        using var context = new BibliothequeContext();
        try
        {
            var categories = context.Categories
                .Include(c => c.LivreCategories)
                .OrderBy(c => c.Nom)
                .AsNoTracking()
                .ToList();

            CategoriesCollectionView.ItemsSource = categories;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
        }
    }

    private async void OnCategorieSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedCategorie = e.CurrentSelection.FirstOrDefault() as Categorie;
        if (selectedCategorie != null)
        {
            using var context = new BibliothequeContext();
            var categorieDetails = await context.Categories
                .Include(c => c.LivreCategories)
                .FirstOrDefaultAsync(c => c.Id == selectedCategorie.Id);

            if (categorieDetails != null)
            {
                NomCategorieEntry.Text = categorieDetails.Nom;
                DescriptionCategorieEntry.Text = categorieDetails.Description;
                FormTitle.Text = "Modifier / Supprimer";
                ModifierButton.IsVisible = true;
                AjouterButton.IsVisible = false;
                SupprimerButton.IsVisible = true;
                FormulaireCategorie.IsVisible = true;
            }
        }
    }

    private async void FiltrerCategories(string searchText)
    {
        using var context = new BibliothequeContext();
        try
        {
            var query = context.Categories
                .Include(c => c.LivreCategories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(c => c.Nom.Contains(searchText));
            }

            var categories = await query
                .OrderBy(c => c.Nom)
                .AsNoTracking()
                .ToListAsync();

            CategoriesCollectionView.ItemsSource = categories;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de filtrer : {ex.Message}", "OK");
        }
    }

    private void OnSearchClicked(object? sender, EventArgs e)
    {
        FiltrerCategories(SearchEntry.Text);
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length >= 3)
        {
            FiltrerCategories(e.NewTextValue);
        }
    }

    private async void OnModifierCategorieClicked(object sender, EventArgs e)
    {
        var selectedCategorie = CategoriesCollectionView.SelectedItem as Categorie;
        if (selectedCategorie == null) return;

        var nom = NomCategorieEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(nom))
        {
            await DisplayAlert("Erreur", "Le nom est obligatoire.", "OK");
            return;
        }

        using var context = new BibliothequeContext();
        // Vérifier unicité (sauf si c'est le même nom)
        var existeDeja = await context.Categories
            .AnyAsync(c => c.Nom == nom && c.Id != selectedCategorie.Id);

        if (existeDeja)
        {
            await DisplayAlert("Erreur", "Une autre catégorie porte déjà ce nom.", "OK");
            return;
        }

        var categorie = await context.Categories.FindAsync(selectedCategorie.Id);
        if (categorie != null)
        {
            categorie.Nom = nom;
            categorie.Description = DescriptionCategorieEntry.Text?.Trim() ?? string.Empty;
            await context.SaveChangesAsync();
            await DisplayAlert("Succès", "Catégorie modifiée !", "OK");
            ChargerCategories();
            ReinitialiserFormulaire();
            FormulaireCategorie.IsVisible = false;
        }
    }

    private async void OnAjouterCategorieClicked(object sender, EventArgs e)
    {
        var nom = NomCategorieEntry.Text?.Trim();
        if (string.IsNullOrWhiteSpace(nom))
        {
            await DisplayAlert("Erreur", "Le nom de la catégorie est obligatoire.", "OK");
            return;
        }

        using var context = new BibliothequeContext();
        if (await context.Categories.AnyAsync(c => c.Nom == nom))
        {
            await DisplayAlert("Erreur", $"La catégorie '{nom}' existe déjà.", "OK");
            return;
        }

        var newCategorie = new Categorie
        {
            Nom = nom,
            Description = DescriptionCategorieEntry.Text?.Trim() ?? string.Empty
        };

        context.Categories.Add(newCategorie);
        await context.SaveChangesAsync();

        await DisplayAlert("Succès", "Catégorie ajoutée avec succès !", "OK");
        ChargerCategories();
        ReinitialiserFormulaire();
        FormulaireCategorie.IsVisible = false;
    }

    private async void OnSupprimerCategorieClicked(object sender, EventArgs e)
    {
        var selectedCategorie = CategoriesCollectionView.SelectedItem as Categorie;
        if (selectedCategorie != null)
        {
            bool confirmed = await DisplayAlert(
                "Confirmer",
                $"Voulez-vous supprimer la catégorie '{selectedCategorie.Nom}' ?",
                "Oui", "Non");

            if (confirmed)
            {
                using var context = new BibliothequeContext();
                var categorie = await context.Categories.FindAsync(selectedCategorie.Id);
                if (categorie != null)
                {
                    context.Categories.Remove(categorie);
                    await context.SaveChangesAsync();
                    await DisplayAlert("Succès", "Catégorie supprimée !", "OK");
                    ChargerCategories();
                    ReinitialiserFormulaire();
                    FormulaireCategorie.IsVisible = false;
                    CategoriesCollectionView.SelectedItem = null;
                }
            }
        }
    }

    private void OnAnnulerCategorieClicked(object sender, EventArgs e)
    {
        FormulaireCategorie.IsVisible = false;
    }
}