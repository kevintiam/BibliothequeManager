using BibliothequeManager.Models;
using BibliothequeManager.Pages.ActionPage;
using BibliothequeManager.Pages.Popups;
using BibliothequeManager.Properties;
using BibliothequeManager.Services;
using BibliothequeManager.Views;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading;

namespace BibliothequeManager.Pages.Views;

public partial class Accueil : ContentPage
{
    private readonly SessionUser session;
    public List<LivreAffichage> LivresDecouverte { get; set; } = new();

    public Accueil(SessionUser user)
    {
        InitializeComponent();
        session = user;

        if (!session.EstConnecte)
        {
            Application.Current.MainPage = new NavigationPage(new Connexion());
            return;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        SetRandomWelcomeQuote();
        await ChargerLivresAleatoiresAsync(); 
    }


    private async Task NavigationToPage<TPage>(object parameter) where TPage : Page
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var page = Activator.CreateInstance(typeof(TPage), parameter) as TPage;
            if (page != null)
            {
                await Navigation.PushAsync(page);
            }
        }
        catch (Exception ex)
        {
            await ErrorPopup.Show("Une erreur est survenue lors de l'ouverture de la page.", this);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnEmprunterLivreClicked(object sender, EventArgs e) => await NavigationToPage<EmpruntPage>(session); 

    private async void OnRetournerLivreClicked(object sender, EventArgs e) => await NavigationToPage<Retour>(session);

    private async void OnReserverLivreClicked(object sender, EventArgs e) => await NavigationToPage<Reserver>(session);

    private async void OnGestionEmpruntsClicked(object sender, EventArgs e) => await NavigationToPage<GestionEmprunts>(session);

    private async void OnGestionReservationsClicked(object sender, EventArgs e) => await NavigationToPage<GestionReservations>(session);

    private async void OnGestionAdherentsClicked(object sender, EventArgs e) => await NavigationToPage<GestionAdherent>(session);

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

        WelcomeSubtitleLabel.Text = quotes[_random.Next(quotes.Length)];
    }

    private async Task ChargerLivresAleatoiresAsync()
    {
        try
        {
            LivresDecouverte.Clear();

            using var context = new BibliothequeContext();
            var livreIds = await context.Livres
                .Select(l => l.Id)
                .ToListAsync();

            var randomIds = livreIds.OrderBy(x => _random.Next()).Take(3).ToList();

            var livres = await context.Livres
                .Include(l => l.Auteur)
                .Include(l => l.Exemplaires)
                .Where(l => randomIds.Contains(l.Id))
                .ToListAsync();

            foreach (var l in livres)
            {
                LivresDecouverte.Add(new LivreAffichage
                {
                    Titre = l.Titre,
                    Auteur = l.Auteur.NomComplet,
                    Description = LivreAffichage.Descriptions[_random.Next(LivreAffichage.Descriptions.Length)],
                    ImageUrl = "book.png"
                });
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                BindableLayout.SetItemsSource(LivresStack, LivresDecouverte);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du chargement des livres: {ex.Message}");
        }
    }
}