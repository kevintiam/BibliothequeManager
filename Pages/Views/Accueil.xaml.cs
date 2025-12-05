using System.Globalization;
using System.Threading;
using BibliothequeManager.Pages.ActionPage;
using BibliothequeManager.Properties;


namespace BibliothequeManager.Pages.Views;

public partial class Accueil : ContentPage
{
    public Accueil()
    {
        InitializeComponent();
      
    }

    // Met à jour la citation de bienvenue à chaque apparition de la page
    protected override void OnAppearing()
    {
        base.OnAppearing();
   
        SetRandomWelcomeQuote();
    }

    private async Task NavigationToPage<TPage>() where TPage : Page, new()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await Navigation.PushAsync(new TPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation échouée", "Une erreur est survenue lors de l'ouverture de la page.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnEmprunterLivreClicked(object sender, EventArgs e)
    {
        await NavigationToPage<EmpruntPage>();
    }

    private async void OnRetournerLivreClicked(object sender, EventArgs e)
    {
        await NavigationToPage<Retour>();
    }

    private async void OnReserverLivreClicked(object sender, EventArgs e)
    {
        await NavigationToPage<Reserver>();
    }


    private async void OnGestionEmpruntsClicked(object sender, EventArgs e)
    {
        await NavigationToPage<GestionEmprunts>();
    }
    private async void OnGestionReservationsClicked(object sender, EventArgs e)
    {
        await NavigationToPage<GestionReservations>();
    }

    private async void OnGestionAdherentsClicked(object sender, EventArgs e)
    {
        await NavigationToPage<GestionAdherent>();
    }
    // Citation aléatoire
    private static readonly Random _random = new Random();

    private void SetRandomWelcomeQuote()
    {
        var quotes = new[]
        {
        AppResources.WelcomeSubtitle_1,
        AppResources.WelcomeSubtitle_2,
        AppResources.WelcomeSubtitle_3,
        AppResources.WelcomeSubtitle_4,
        AppResources.WelcomeSubtitle_5,
        AppResources.WelcomeSubtitle_6
    };
        // Sélectionne une citation aléatoire
        WelcomeSubtitleLabel.Text = quotes[_random.Next(quotes.Length)];
    }




}
