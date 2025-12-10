using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using Microsoft.EntityFrameworkCore;
using BibliothequeManager.Pages.Views;
using Microsoft.Maui.Controls;
using BibliothequeManager.Services;
using BibliothequeManager.Views;

namespace BibliothequeManager.Pages.ActionPage
{

    public partial class Reserver : ContentPage
    {
        private readonly SessionUser session;
        private int? livreSelectionne;
        public Reserver(SessionUser user)
        {
            InitializeComponent();
            session = user;

            if (!session.EstConnecte)
            {
                Application.Current.MainPage = new NavigationPage(new Connexion());
                return;
            }
                SearchButton.Clicked += OnRechercherClicked;
                StatutPicker.ItemsSource = StatutOptions;
                SuggestionsCollectionView.SelectionChanged += OnLivreSuggestionSelected;

                RechercheEntry.TextChanged += OnSearchTextChanged;
        }
        /// <summary>
        /// liste des options de statut pour le filtre
        /// </summary>
        public List<string> StatutOptions { get; } = new()
        {
            App.Localized["All"],
            App.Localized["Pending"],
            App.Localized["Confirmed"],
            App.Localized["InProgress"],
            App.Localized["Returned"],
            App.Localized["Cancel"]
        };
        /// <summary>
        /// Filtrer les livres en fonction du texte de recherche
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
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

            }
            catch (Exception ex)
            {
                string messageUtilisateur = "Impossible de charger les livres. Veuillez réessayer." + ex.Message;
                await ErrorPopup.Show(messageUtilisateur, this);
                SuggestionsCollectionView.IsVisible = false;
            }
        }
        /// <summary>
        /// Gérer le changement de texte dans la barre de recherche
        /// </summary>
        private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Length >= 3)
            {
                await FiltrerLivre(e.NewTextValue);
            }
        }
        /// <summary>
        ///  Bouton de recherche cliqué
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnRechercherClicked(object? sender, EventArgs e)
        {
            await FiltrerLivre(RechercheEntry.Text);
        }
        /// <summary>
        /// Fonction appelée lors de la sélection d'un livre dans les suggestions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnLivreSuggestionSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Livres livre)
            {
                livreSelectionne = livre.Id;
                LivreSection.IsVisible = true;
                TitreLivre.Text = livre.Titre;
                IsbnLivre.Text = livre.ISBN;
                AuteurLivre.Text = $"{livre.Auteur?.Prenom} {livre.Auteur?.Nom}".Trim();

                bool estDisponible = livre.Exemplaires?.Any(ex => ex.EstDisponible) ?? false;
                var exemplairesDisponibles = livre.Exemplaires?.Count(e => e.EstDisponible) ?? 0;

                DisponibiliteLivre.Text = estDisponible ? $"Disponible ({exemplairesDisponibles} exemplaire(s))" : "Indisponible";
                DisponibiliteLivre.TextColor = estDisponible ? Colors.Green : Colors.Red;
                SuggestionsCollectionView.IsVisible = false;
                SuggestionsCollectionView.SelectedItem = null;
                RechercheEntry.Unfocus();
            }
        }
        /// <summary>
        /// Fonction appelée lors du clic sur le bouton Confirmer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            using var donnees = new BibliothequeContext();
            using var transaction = await donnees.Database.BeginTransactionAsync();
            try
            {
                // Verifier si l'abonnee existe
                var abonne = await donnees.Adherents
                    .FirstOrDefaultAsync(a => a.NumeroCarte == AbonneIdEntry.Text);

                if (abonne == null)
                {
                    await ErrorPopup.Show("Adhérent introuvable.", this);
                    return;
                }

                // Verifier le nombre de reservations en cours
                var reservationsEnCours = await donnees.Reservations
                    .CountAsync(r => r.AdherentId == abonne.Id &&
                        r.Statut != "Annulée" &&
                        r.Statut != "Terminée");

                const int limiteReservations = 5; // Limite configurable
                if (reservationsEnCours >= limiteReservations)
                {
                    await ErrorPopup.Show($"L'adhérent a déjà {limiteReservations} réservations en cours.",this);
                    return;
                }

                // Verifier si le livre existe
                var livre = await donnees.Livres.FindAsync(livreSelectionne.Value);
                if (livre == null)
                {
                    await ErrorPopup.Show("Livre introuvable.", this);
                    return;
                }

                // Trouver un exemplaire éligible à la réservation
                var exemplaireDisponible = await donnees.Exemplaires
                .FirstOrDefaultAsync(e => e.LivreId == livreSelectionne.Value && e.EstDisponible);

                if (exemplaireDisponible == null)
                {
                    await ErrorPopup.Show("Aucun exemplaire disponible pour ce livre.", this);
                    return;
                }
                var reservationExistante = await donnees.Reservations
                    .AnyAsync(r =>
                        r.AdherentId == abonne.Id &&
                        r.LivreId == livreSelectionne.Value &&
                        (r.Statut == "En Attente" || r.Statut == "Confirmée"));

                if (reservationExistante) {
                    await ErrorPopup.Show("L'adhérent a déjà une réservation en cours pour ce livre.", this);
                    return;

                }

                var nombreReservationsEnAttente = await donnees.Reservations
                    .CountAsync(r => 
                        r.LivreId == livreSelectionne.Value && 
                        r.Statut == "En Attente");

                var priorite = nombreReservationsEnAttente > 2 ? "Haute" : "Normale";
                // Définir les dates par défaut
                DateTime dateFin = DateFinPicker.Date;

                // Créer la réservation
                var newReservation = new Reservation
                {
                    LivreId = livreSelectionne.Value,
                    AdherentId = abonne.Id,
                    DateReservation = DateTime.UtcNow,
                    BibliothecaireId = session.UtilisateurActuel.Id,
                    ExemplaireAttribueId = exemplaireDisponible.Id,
                    DateRecuperationPrevue = DateDebutPicker.Date,
                    Statut = "En Attente",
                    Priorite = priorite,
                };
                exemplaireDisponible.EstDisponible = false;

                donnees.Reservations.Add(newReservation);
                await donnees.SaveChangesAsync();
                await transaction.CommitAsync();

                await SuccessPopup.Show("Réservation confirmée avec succès !", this);

                await ReinitialiserFormulaire();
            }
            catch (Exception ex)
            {   
                var inner = ex.InnerException?.Message;
                var message = inner != null ? $"Erreur lors de la réservation : {inner}" : $"Erreur lors de la réservation : {ex.Message}";
                await transaction.RollbackAsync();
                await ErrorPopup.Show(message, this);
            }
        }
        /// <summary>
        /// Réinitialiser le formulaire de réservation
        /// </summary>
        /// <returns></returns>
        private async Task ReinitialiserFormulaire()
        {
            // Réinitialiser les champs
            LivreSection.IsVisible = false;
            RechercheEntry.Text = string.Empty;
            AbonneIdEntry.Text = string.Empty;
            livreSelectionne = null;
            DateDebutPicker.Date = DateTime.Today;
            DateFinPicker.Date = DateTime.Today.AddDays(7);

            // Masquer les suggestions
            SuggestionsCollectionView.IsVisible = false;
            // Retourner au focus initial
            RechercheEntry.Focus();
        }
        /// <summary>
        /// Bouton Accueil cliqué
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAccueilClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
        /// <summary>
        /// Bouton Mes Réservations cliqué
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnMesReservationsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GestionReservations(session));
        }

        // <summary>
        /// Gestion de l'apparition de la page
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            RechercheEntry.Focus();
        }
    }
}