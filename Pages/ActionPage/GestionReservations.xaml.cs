namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionReservations : ContentPage
{
	public GestionReservations()
	{
		InitializeComponent();
	}

	private async void OnAccueilClicked(object sender, EventArgs e)
	{
		await Navigation.PushModalAsync(new HomePage());
	}
	private async void OnNewReservation(object sender,EventArgs e)
	{
		await Navigation.PushModalAsync(new Reserver());
	}
}