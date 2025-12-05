using BibliothequeManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionReservations : ContentPage
{
	public GestionReservations()
	{
		InitializeComponent();
        StatutPicker.ItemsSource = StatutOptions;
		ChargerReservation();

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
	private async void OnAccueilClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new HomePage());
	}
	private async void OnNewReservation(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Reserver());
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
}