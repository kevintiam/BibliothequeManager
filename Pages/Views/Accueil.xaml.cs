using BibliothequeManager.Pages.ActionPage;

namespace BibliothequeManager.Pages.Views;

public partial class Accueil : ContentPage
{
    public Accueil()
    {
        InitializeComponent();
    }

    private async void OnEmprunterLivreClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Emprunt());
    }
    private async void OnRetournerLivreClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Retour());
    }
    private async void OnReserverLivreClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Reserver());
    }
    private async void OnGestionEmpruntsClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new GestionEmprunts());
    }
    private async void OnGestionReservationsClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new GestionReservations());
    }
    private async void OnGestionAdherents(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Adherent());
    }
}