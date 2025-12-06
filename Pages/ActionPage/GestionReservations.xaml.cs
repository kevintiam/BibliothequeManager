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
    public ICommand Valider { get; }
    public ICommand Modifier { get; }

    public GestionReservations(SessionUser user)
	{
		InitializeComponent();
        session = user;
        if(!session.EstConnecte)
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

	private async void OnValider (Reservation reservation)
	{
		if(reservation != null)
		{
			using var context = new BibliothequeContext();
			var reservationDB = context.Reservations.First(r => r.Id == reservation.Id);
			reservationDB.Statut = StatutsReservation.Confirmee;
			context.SaveChanges();
            await SuccessPopup.Show("Réservation confirmée avec succès.", this);
            ChargerReservation();
            ChargerStatistique();

        }
	}

    private async void OnModifier(Reservation reservation)
    {
        if (reservation == null) return;

        using var context = new BibliothequeContext();
        var reservationDB = await context.Reservations
            .FirstAsync(r => r.Id == reservation.Id);

            reservationDB.Statut = StatutsReservation.EnCours;

        var newEmprunt = new Emprunt
        {
            AdherentId = reservationDB.AdherentId,
            DateEmprunt = reservationDB.DateRecuperationPrevue,
            DateRetourPrevu = reservationDB.DateRecuperationPrevue.AddDays(14),
            BibliothecaireEmpruntId = session.UtilisateurActuel.Id,
            ExemplaireId = reservationDB.ExemplaireAttribueId.Value
        };
        context.Emprunts.Add(newEmprunt);

        await context.SaveChangesAsync();
        await SuccessPopup.Show("Réservation convertie en emprunt.", this);
        ChargerReservation();
        ChargerStatistique();
    }

    private void ChargerReservation()
	{
		using var donnee = new BibliothequeContext();

		var reservations = donnee.Reservations
			.Include(r => r.Livre)
			.Include(r => r.Adherent)
			.Include(r => r.ExemplaireAttribue)
			.ToList();

		CollectionViewReservations.ItemsSource = reservations;
    }

    private void ChargerStatistique()
    {
        using var contexte = new BibliothequeContext();
        var enAttente = contexte.Reservations.Count(r => r.Statut == StatutsReservation.EnAttente);
        var confirmee = contexte.Reservations.Count(r => r.Statut == StatutsReservation.Confirmee);
        var enCours = contexte.Reservations.Count(r => r.Statut == StatutsReservation.EnCours);
        var enRetard = contexte.Reservations.Count(r => r.Statut == StatutsReservation.EnRetard);
        var annulee = contexte.Reservations.Count(r => r.Statut == StatutsReservation.Annulee);

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
	private async void OnNewReservation(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Reserver(session));
	}
    public List<string> StatutOptions { get; } = new()
	{
		App.Localized["All"],
		App.Localized["Pending"],
		App.Localized["Confirmed"],
		App.Localized["InProgress"],
		App.Localized["Returned"],
		App.Localized["Cancel"]
	};

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
            await DisplayAlert("Erreur", $"Impossible de filtrer : {ex.Message}", "OK");
        }
    }

    private void OnSearchBarTextChanged(object? sender, TextChangedEventArgs e)
    {
        FiltrerReservations(e.NewTextValue);
    }

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