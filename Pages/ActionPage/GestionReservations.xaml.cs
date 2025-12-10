using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using BibliothequeManager.Services;
using BibliothequeManager.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using static BibliothequeManager.Models.Reservation;

namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionReservations : ContentPage
{
    private readonly SessionUser session;
    /// <summary>
    /// Commande pour valider une réservation
    /// </summary>
    public ICommand Valider { get; }
    /// <summary>
    /// Commande pour modifier une réservation
    /// </summary>
    public ICommand Modifier { get; }

    public GestionReservations(SessionUser user)
    {
        InitializeComponent();
        session = user;
        if (!session.EstConnecte)
        {
            Application.Current.MainPage = new NavigationPage(new Connexion());
            return;
        }
        StatutPicker.ItemsSource = StatutOptions;

        Valider = new Command<Reservation>(OnValider);
        Modifier = new Command<Reservation>(OnModifier);

        BindingContext = this;

        SearchEntry.TextChanged += OnSearchBarTextChanged;

        ChargerReservation();
        ChargerStatistique();
    }

    /// <summary>
    /// Bouton pour valider une réservation
    /// </summary>
    /// <param name="reservation"></param>
    private async void OnValider(Reservation reservation)
    {
        if (reservation != null)
        {
            using var context = new BibliothequeContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var reservationDB = await context.Reservations
                    .FirstAsync(r => r.Id == reservation.Id);

                reservationDB.Statut = StatutsReservation.Confirmee;
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                await SuccessPopup.Show("Réservation confirmée avec succès.", this);
                await ChargerReservation();
                await ChargerStatistique();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await ErrorPopup.Show($"Erreur lors de la confirmation : {ex.Message}", this);
            }
        }
    }

    /// <summary>
    /// Bouton pour modifier une réservation en emprunt
    /// </summary>
    /// <param name="reservation"></param>
    private async void OnModifier(Reservation reservation)
    {
        if (reservation == null) return;

        using var context = new BibliothequeContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var reservationDB = await context.Reservations
                .Include(r => r.ExemplaireAttribue)
                .FirstAsync(r => r.Id == reservation.Id);

            if (reservationDB.Statut != StatutsReservation.Confirmee)
            {
                await ErrorPopup.Show("Seules les réservations confirmées peuvent être converties en emprunt.", this);
                return;
            }

            if (!reservationDB.ExemplaireAttribueId.HasValue)
            {
                await ErrorPopup.Show("Aucun exemplaire attribué à cette réservation.", this);
                return;
            }

            reservationDB.Statut = StatutsReservation.EnCours;

            var newEmprunt = new Emprunt
            {
                AdherentId = reservationDB.AdherentId,
                DateEmprunt = reservationDB.DateRecuperationPrevue,
                DateRetourPrevu = reservationDB.DateRecuperationPrevue.AddDays(14),
                BibliothecaireEmpruntId = session.UtilisateurActuel.Id,
                ExemplaireId = reservationDB.ExemplaireAttribueId.Value,
            };

            context.Emprunts.Add(newEmprunt);
            if (reservationDB.ExemplaireAttribue != null)
            {
                reservationDB.ExemplaireAttribue.EstDisponible = false;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            await SuccessPopup.Show("Réservation convertie en emprunt.", this);
            await ChargerReservation();
            await ChargerStatistique();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await ErrorPopup.Show($"Erreur : {ex.Message}", this);
        }
    }

    /// <summary>
    /// Fonction pour charger les réservations
    /// </summary>
    /// <returns></returns>
    private async Task ChargerReservation()
    {
        using var donnee = new BibliothequeContext();

        var reservations = await donnee.Reservations
            .Include(r => r.Livre)
            .Include(r => r.Adherent)
            .Include(r => r.ExemplaireAttribue)
            .ToListAsync();

        CollectionViewReservations.ItemsSource = reservations;
    }

    /// <summary>
    /// Fonction pour charger les statistiques des réservations
    /// </summary>
    /// <returns></returns>
    private async Task ChargerStatistique()
    {
        using var contexte = new BibliothequeContext();
        var enAttente = await contexte.Reservations.CountAsync(r => r.Statut == StatutsReservation.EnAttente);
        var confirmee = await contexte.Reservations.CountAsync(r => r.Statut == StatutsReservation.Confirmee);
        var enCours = await contexte.Reservations.CountAsync(r => r.Statut == StatutsReservation.EnCours);
        var enRetard = await contexte.Reservations.CountAsync(r => r.Statut == StatutsReservation.EnRetard);
        var annulee = await contexte.Reservations.CountAsync(r => r.Statut == StatutsReservation.Annulee);

        ReservationsEnAttente.Text = enAttente.ToString();
        ReservationsConfirmees.Text = confirmee.ToString();
        ReservationsEnCours.Text = enCours.ToString();
        ReservationsExpirees.Text = enRetard.ToString();
        TotalReservations.Text = (enAttente + confirmee + enCours + enRetard + annulee).ToString();
    }

    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        //await Navigation.PushAsync(new HomePage());
    }

    /// <summary>
    /// Bouton pour créer une nouvelle réservation
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnNewReservation(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Reserver(session));
    }

    /// <summary>
    /// Liste des options de statut pour le filtre
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
    /// Fonction pour filtrer les réservations
    /// </summary>
    /// <param name="searchText"></param>
    private async void FiltrerReservations(string searchText)
    {
        using var context = new BibliothequeContext();
        try
        {
            var query = context.Reservations
                .Include(l => l.Adherent)
                .Include(l => l.Livre)
                .Include(l => l.ExemplaireAttribue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(l =>
                    l.Livre.Titre.Contains(searchText) ||
                    l.Livre.ISBN.Contains(searchText) ||
                    (l.Adherent != null && l.Adherent.Nom.Contains(searchText)) ||
                    (l.Adherent != null && l.Adherent.Prenom.Contains(searchText)));
            }

            var reservations = await query
                .OrderBy(l => l.Livre.Titre)
                .AsNoTracking()
                .ToListAsync();

            CollectionViewReservations.ItemsSource = reservations;
        }
        catch (Exception ex)
        {
            await ErrorPopup.Show($"Impossible de filtrer : {ex.Message}", this);
        }
    }

    /// <summary>
    /// Barre de recherche en temps réel
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSearchBarTextChanged(object? sender, TextChangedEventArgs e)
    {
        FiltrerReservations(e.NewTextValue);
    }

    /// <summary>
    /// Logique pour le filtre par statut
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StatutPicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        using var donnee = new BibliothequeContext();

        string? statutSelectionne = null;
        if (StatutPicker.SelectedIndex > 0)
        {
            var optionAffichee = StatutPicker.SelectedItem?.ToString();
            statutSelectionne = optionAffichee switch
            {
                var s when s == App.Localized["Pending"] => StatutsReservation.EnAttente,
                var s when s == App.Localized["Confirmed"] => StatutsReservation.Confirmee,
                var s when s == App.Localized["InProgress"] => StatutsReservation.EnCours,
                var s when s == App.Localized["Overdue"] => StatutsReservation.EnRetard,
                var s when s == App.Localized["Cancel"] => StatutsReservation.Annulee,
                _ => null
            };
        }
        var query = donnee.Reservations
            .Include(r => r.Livre)
            .Include(r => r.Adherent)
            .Include(r => r.ExemplaireAttribue)
            .AsQueryable();

        if (!string.IsNullOrEmpty(statutSelectionne))
        {
            query = query.Where(r => r.Statut == statutSelectionne);
        }

        var reservations = query.ToList();
        CollectionViewReservations.ItemsSource = reservations;
    }
}