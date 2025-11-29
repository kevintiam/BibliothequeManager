using System.Globalization;
using System.Threading;
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
        await Navigation.PushAsync(new Emprunt());
    }

    private async void OnRetournerLivreClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Retour());
    }

    private async void OnReserverLivreClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Reserver());
    }

    private async void OnGestionEmpruntsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GestionEmprunts());
    }

    private async void OnGestionReservationsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new GestionReservations());
    }

    private async void OnGestionAdherents(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Adherent());
    }
}