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

 

    // Bascule la langue entre français et anglais, puis recharge la page   

    private  void OnSwitchLanguageClicked(object sender, EventArgs e)
    {
        // Détermine la langue actuelle
        string langueActuelle = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        // Bascule vers l'autre langue
        string nouvelleLangue = langueActuelle == "fr" ? "en" : "fr";

        // Crée la culture correspondante
        var culture = new CultureInfo(nouvelleLangue);

        // Applique cette culture partout dans l'application
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        // Recharge la page : on la retire, puis on la remet
        App.Localized.OnCultureChanged();
    }

 

    private async void OnEmprunterLivreClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Emprunt());
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