using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using BibliothequeManager.Pages.Views;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Pages.ActionPage;

public partial class EmpruntPage : ContentPage
{
    private int? livreSelectionne;
    public EmpruntPage()
	{
		InitializeComponent();
        SearchButton.Clicked += OnRechercherClicked;
        SuggestionsCollectionView.SelectionChanged += OnLivreSuggestionSelected;

        RechercheEntry.TextChanged += async (s,e) => {
            await Task.Delay(300); 

            if(e.NewTextValue == RechercheEntry.Text)
            {
                await OnSearchTextChanged(s, e);
            }
            
        };
    }
    
    private async void OnConfirmerClicked(object sender, EventArgs e)
    {
        // Validation
        if (!livreSelectionne.HasValue)
        {
            await ErrorPopup.Show("Veuillez sélectionner un livre.", this);
            return;
        }

        if (string.IsNullOrWhiteSpace(AbonneIdEntry.Text))
        {
            await ErrorPopup.Show("Veuillez entrer le numero de carte de l'adhérent.", this);
            return;
        }

        try
        {
            using var donnees = new BibliothequeContext();
            // Verifier si l'abonnee existe
            var abonne = await donnees.Adherents
                .FirstOrDefaultAsync(a => a.NumeroCarte == AbonneIdEntry.Text);

            if (abonne == null)
            {
                await ErrorPopup.Show("Adhérent introuvable.", this);
                return;
            }
            // Verifier si le livre existe
            var livre = await donnees.Livres.FindAsync(livreSelectionne.Value);
            if (livre == null)
            {
                await ErrorPopup.Show("Livre introuvable.", this);
                return;

            }

            // Trouver un exemplaire éligible à l'emprunt
            var exemplaireDisponible = await donnees.Exemplaires
            .FirstOrDefaultAsync(e => e.LivreId == livreSelectionne.Value && e.EstDisponible);

            


            // Créer l'emprunt
            var newEmprunt = new Emprunt
            {
                AdherentId = abonne.Id,
                DateEmprunt = DateTime.UtcNow,
                DateRetourPrevu = DateTime.UtcNow.AddDays(14),
                BibliothecaireEmpruntId = 1,
                ExemplaireId = exemplaireDisponible.Id
            };


            donnees.Emprunts.Add(newEmprunt);
            await donnees.SaveChangesAsync();


            await SuccessPopup.Show("Emprunt confirmé avec succès !", this);

            // Réinitialiser le formulaire
            LivreSection.IsVisible = false;
            RechercheEntry.Text = string.Empty;
            AbonneIdEntry.Text = string.Empty;
            livreSelectionne = null;
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message;
            var message = inner != null ? $"Erreur lors de la réservation : {inner}" : $"Erreur lors de la réservation : {ex.Message}";
            await ErrorPopup.Show(message, this);
        }
    }

    private async Task FiltrerLivre(string searchText)
    {
        using var donnees = new BibliothequeContext();
        try
        {
            var query = donnees.Livres
                .Include(l => l.Auteur)
                .Include(l => l.Exemplaires)
                .Include(l => l.LivreCategories)
                    .ThenInclude(lc => lc.Categorie)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(l =>
                    l.Titre.Contains(searchText) ||
                    l.ISBN.Contains(searchText) ||
                    (l.Auteur != null && (
                        l.Auteur.Nom.Contains(searchText) ||
                        l.Auteur.Prenom.Contains(searchText)
                    ))
                );
            }

            var livresFiltres = await query
                .OrderBy(l => l.Titre)
                .ToListAsync();

            SuggestionsCollectionView.ItemsSource = livresFiltres;
            SuggestionsCollectionView.IsVisible = livresFiltres.Any();

            // Optionnel : afficher un message si aucun livre trouvé
            // (vous pouvez ajouter un Label dans votre XAML nommé "NoResultsLabel")
            // NoResultsLabel.IsVisible = !livresFiltres.Any();
        }
        catch (Exception ex)
        {
            string messageUtilisateur = "Impossible de charger les livres. Veuillez réessayer.";
            await ErrorPopup.Show(messageUtilisateur, this);
            SuggestionsCollectionView.IsVisible = false;
        }
    }

    private async Task OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length >= 3)
        {
            await FiltrerLivre(e.NewTextValue);
        }
    }

    private async void OnRechercherClicked(object? sender, EventArgs e)
    {
        await FiltrerLivre(RechercheEntry.Text);
    }

    private async void OnLivreSuggestionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if( e.CurrentSelection.FirstOrDefault() is Livres livre)
        {
            livreSelectionne = livre.Id;
            LivreSection.IsVisible = true;
            TitreLivre.Text = livre.Titre;
            IsbnLivre.Text = livre.ISBN;
            AuteurLivre.Text = $"{livre.Auteur?.Prenom} {livre.Auteur?.Nom}".Trim();

            bool estDisponible = livre.Exemplaires?.Any(ex => ex.EstDisponible) ?? false;
            DisponibiliteLivre.Text = estDisponible ? "Disponible" : "Indisponible";

            SuggestionsCollectionView.IsVisible = false;
            SuggestionsCollectionView.SelectedItem = null;
            RechercheEntry.Unfocus();
        }
    }
    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        // Navigation vers la page d'accueil
        await Navigation.PopAsync();
    }

    private async void OnListeEmpruntsClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new GestionEmprunts());

    }
}
