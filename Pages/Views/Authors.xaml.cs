using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using BibliothequeManager.Services;
using BibliothequeManager.Views;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Pages.Views;

public partial class Authors : ContentPage
{
    // Session utilisateur pour gérer l'authentification
    public readonly SessionUser session;

    /// <summary>
    /// Constructeur de la page de gestion des auteurs
    /// </summary>
    /// <param name="user">Session utilisateur actuelle</param>
    public Authors(SessionUser user)
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
        AuthorsCollectionView.SelectionChanged += OnAuthorSelectionChanged;

        // Charger la liste initiale des auteurs
        ChargerAuteurs();
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
            FormTitle.Text = App.Localized["AddOthor"];
            ModifierButton.IsVisible = false;
            AjouterButton.IsVisible = true;
            FormulaireLivres.IsVisible = true;
            SupprimerButton.IsVisible = false;
        }
    }

    /// <summary>
    /// Réinitialise les champs du formulaire à leurs valeurs par défaut
    /// </summary>
    private void ReinitialiserFormulaire()
    {
        PrenomEntry.Text = "";
        NomEntry.Text = "";
    }

    /// <summary>
    /// Charge la liste des auteurs depuis la base de données
    /// </summary>
    private void ChargerAuteurs()
    {
        using var Donnees = new BibliothequeContext();
        try
        {
            // Récupérer les auteurs avec leurs livres associés
            var auteurs = Donnees.Auteurs
                .Include(a => a.Livres)
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .AsNoTracking() // Optimisation : lecture seule
                .ToList();

            // Afficher la liste dans la CollectionView
            AuthorsCollectionView.ItemsSource = auteurs;
        }
        catch (Exception ex)
        {
            // Journaliser l'erreur
            Console.WriteLine($"Erreur : {ex.Message}");
            // Note: En production, utiliser un système de logging approprié
        }
    }

    /// <summary>
    /// Gère la sélection d'un auteur dans la liste
    /// </summary>
    private async void OnAuthorSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Récupérer l'auteur sélectionné
        var selectedAuthor = e.CurrentSelection.FirstOrDefault() as Auteur;

        if (selectedAuthor != null)
        {
            int authorId = selectedAuthor.Id;

            // Charger les détails complets de l'auteur depuis la base de données
            using var Donnees = new BibliothequeContext();
            var auteurDetails = await Donnees.Auteurs
                .FirstOrDefaultAsync(a => a.Id == authorId);

            if (auteurDetails != null)
            {
                // Remplir le formulaire avec les données de l'auteur
                PrenomEntry.Text = auteurDetails.Prenom;
                NomEntry.Text = auteurDetails.Nom;

                // Configurer l'interface pour la modification
                FormTitle.Text = App.Localized["EditOrDelete"];
                ModifierButton.IsVisible = true;
                AjouterButton.IsVisible = false;
                SupprimerButton.IsVisible = true;
                FormulaireLivres.IsVisible = true;
            }
        }
    }

    /// <summary>
    /// Filtre les auteurs en fonction du texte de recherche
    /// </summary>
    /// <param name="searchText">Texte à rechercher</param>
    private async void FiltrerAuteurs(string searchText)
    {
        using var donnees = new BibliothequeContext();
        try
        {
            var query = donnees.Auteurs
                .Include(a => a.Livres)
                .AsQueryable();

            // Appliquer le filtre si un texte est fourni
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(a =>
                    a.Nom.Contains(searchText) ||
                    a.Prenom.Contains(searchText));
            }

            // Exécuter la requête et récupérer les résultats
            var auteurs = await query
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .AsNoTracking() // Optimisation pour les lectures
                .ToListAsync();

            // Mettre à jour la liste affichée
            AuthorsCollectionView.ItemsSource = auteurs;
        }
        catch (Exception ex)
        {
            // Afficher un message d'erreur à l'utilisateur
            await DisplayAlert("Erreur", $"Impossible de filtrer : {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Gère le clic sur le bouton de recherche
    /// </summary>
    private void OnSearchClicked(object? sender, EventArgs e)
    {
        FiltrerAuteurs(SearchEntry.Text);
    }

    /// <summary>
    /// Gère la recherche en temps réel lors de la saisie
    /// </summary>
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        // Délencher la recherche après 3 caractères ou si le champ est vide
        if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length >= 3)
        {
            FiltrerAuteurs(e.NewTextValue);
        }
    }

    /// <summary>
    /// Gère la modification d'un auteur existant
    /// </summary>
    private async void OnModifierClicked(object sender, EventArgs e)
    {
        // Récupérer l'auteur sélectionné
        var selectedAuthor = AuthorsCollectionView.SelectedItem as Auteur;

        if (selectedAuthor != null)
        {
            int authorId = selectedAuthor.Id;

            // Valider les données du formulaire
            var prenom = PrenomEntry.Text?.Trim();
            var nom = NomEntry.Text?.Trim();

            if (string.IsNullOrEmpty(prenom) || string.IsNullOrEmpty(nom))
            {
                await ErrorPopup.Show(App.Localized["popErrorAutor1"], this);
                return;
            }

            // Mettre à jour l'auteur dans la base de données
            using var Donnees = new BibliothequeContext();
            var auteurToUpdate = await Donnees.Auteurs.FindAsync(authorId);

            if (auteurToUpdate != null)
            {
                auteurToUpdate.Prenom = prenom;
                auteurToUpdate.Nom = nom;
                await Donnees.SaveChangesAsync();
                await SuccessPopup.Show(App.Localized["AuthorSuccessfullyModified"], this);

                ChargerAuteurs();
                ReinitialiserFormulaire();

                selectedAuthor = null;
                FormulaireLivres.IsVisible = false;
            }
        }
    }

    /// <summary>
    /// Gère l'ajout d'un nouvel auteur
    /// </summary>
    private async void OnAjouterClicked(object sender, EventArgs e)
    {
        // Valider les données du formulaire
        var prenom = PrenomEntry.Text?.Trim();
        var nom = NomEntry.Text?.Trim();

        if (string.IsNullOrEmpty(prenom) || string.IsNullOrEmpty(nom))
        {
            await ErrorPopup.Show(App.Localized["popErrorAutor1"], this);
            return;
        }

        using var Donnees = new BibliothequeContext();

        // Vérifier si l'auteur existe déjà (prénom ET nom identiques)
        if (await Donnees.Auteurs.AnyAsync(a => a.Prenom == prenom) &&
           (await Donnees.Auteurs.AnyAsync(a => a.Nom == nom)))
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

    /// <summary>
    /// Gère la suppression d'un auteur
    /// </summary>
    private async void OnSupprimerClicked(object sender, EventArgs e)
    {
        // Récupérer l'auteur sélectionné
        var selectedAuthor = AuthorsCollectionView.SelectedItem as Auteur;

        if (selectedAuthor != null)
        {
            // Créer et afficher un popup de confirmation
            var popup = new ConfirmationPopup
            {
                Title = App.Localized["ConfirmDelete"],
                Message = string.Format(App.Localized["popDelAuthor"],
                    selectedAuthor.Prenom, selectedAuthor.Nom)
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

    /// <summary>
    /// Supprime un auteur de la base de données
    /// </summary>
    /// <param name="id">Identifiant de l'auteur à supprimer</param>
    private async Task SupprimerAuteur(int id)
    {
        using var context = new BibliothequeContext();

        // Rechercher l'auteur à supprimer
        var auteur = await context.Auteurs.FindAsync(id);

        if (auteur != null)
        {
            // Supprimer l'auteur
            context.Auteurs.Remove(auteur);
            await context.SaveChangesAsync();

            // Afficher un message de succès
            await SuccessPopup.Show(App.Localized["AuthorSuccessfullyDeleted!"], this);

            // Rafraîchir l'affichage
            ChargerAuteurs();
            ReinitialiserFormulaire();

            // Masquer le formulaire et réinitialiser la sélection
            FormulaireLivres.IsVisible = false;
            AuthorsCollectionView.SelectedItem = null;
        }
    }
}