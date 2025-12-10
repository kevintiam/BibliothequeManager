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
    // Session utilisateur pour gérer l'authentification
    private readonly SessionUser session;

    // Liste des livres à afficher dans la section "Découverte"
    public List<LivreAffichage> LivresDecouverte { get; set; } = new();

    /// <summary>
    /// Constructeur de la page d'accueil
    /// </summary>
    /// <param name="user">Session utilisateur actuelle</param>
    public Accueil(SessionUser user)
    {
        InitializeComponent();
        session = user;

        // Vérifier si l'utilisateur est connecté
        if (!session.EstConnecte)
        {
            // Rediriger vers la page de connexion si non connecté
            Application.Current.MainPage = new NavigationPage(new Connexion());
            return;
        }
    }

    /// <summary>
    /// Méthode exécutée lorsque la page apparaît à l'écran
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Définir une citation d'accueil aléatoire
        SetRandomWelcomeQuote();

        // Charger des livres aléatoires pour la section découverte
        await ChargerLivresAleatoiresAsync();
    }

    /// <summary>
    /// Méthode générique pour naviguer vers une autre page
    /// </summary>
    /// <typeparam name="TPage">Type de la page de destination</typeparam>
    /// <param name="parameter">Paramètre à passer à la page de destination</param>
    private async Task NavigationToPage<TPage>(object parameter) where TPage : Page
    {
        // Éviter les doubles clics
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            // Créer une instance de la page avec le paramètre
            var page = Activator.CreateInstance(typeof(TPage), parameter) as TPage;

            if (page != null)
            {
                // Naviguer vers la page
                await Navigation.PushAsync(page);
            }
        }
        catch (Exception ex)
        {
            // Afficher un message d'erreur en cas d'échec
            await ErrorPopup.Show("Une erreur est survenue lors de l'ouverture de la page.", this);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Gestion du clic sur le bouton "Emprunter un livre"
    /// </summary>
    private async void OnEmprunterLivreClicked(object sender, EventArgs e)
        => await NavigationToPage<EmpruntPage>(session);

    /// <summary>
    /// Gestion du clic sur le bouton "Retourner un livre"
    /// </summary>
    private async void OnRetournerLivreClicked(object sender, EventArgs e)
        => await NavigationToPage<Retour>(session);

    /// <summary>
    /// Gestion du clic sur le bouton "Réserver un livre"
    /// </summary>
    private async void OnReserverLivreClicked(object sender, EventArgs e)
        => await NavigationToPage<Reserver>(session);

    /// <summary>
    /// Gestion du clic sur le bouton "Gestion des emprunts"
    /// </summary>
    private async void OnGestionEmpruntsClicked(object sender, EventArgs e)
        => await NavigationToPage<GestionEmprunts>(session);

    /// <summary>
    /// Gestion du clic sur le bouton "Gestion des réservations"
    /// </summary>
    private async void OnGestionReservationsClicked(object sender, EventArgs e)
        => await NavigationToPage<GestionReservations>(session);

    /// <summary>
    /// Gestion du clic sur le bouton "Gestion des adhérents"
    /// </summary>
    private async void OnGestionAdherentsClicked(object sender, EventArgs e)
        => await NavigationToPage<GestionAdherent>(session);

    // Générateur de nombres aléatoires pour les citations et la sélection de livres
    private static readonly Random _random = new Random();

    /// <summary>
    /// Définit une citation d'accueil aléatoire parmi les ressources disponibles
    /// </summary>
    private void SetRandomWelcomeQuote()
    {
        // Tableau des citations disponibles dans les ressources
        var quotes = new[]
        {
            AppResources.WelcomeSubtitle_1,
            AppResources.WelcomeSubtitle_2,
            AppResources.WelcomeSubtitle_3,
            AppResources.WelcomeSubtitle_4,
            AppResources.WelcomeSubtitle_5,
            AppResources.WelcomeSubtitle_6
        };

        // Sélectionner une citation aléatoire et l'afficher
        WelcomeSubtitleLabel.Text = quotes[_random.Next(quotes.Length)];
    }

    /// <summary>
    /// Charge 3 livres aléatoires pour la section "Découverte"
    /// </summary>
    private async Task ChargerLivresAleatoiresAsync()
    {
        try
        {
            // Vider la liste actuelle
            LivresDecouverte.Clear();

            using var context = new BibliothequeContext();

            // Récupérer tous les IDs de livres
            var livreIds = await context.Livres
                .Select(l => l.Id)
                .ToListAsync();

            // Sélectionner 3 IDs aléatoires
            var randomIds = livreIds.OrderBy(x => _random.Next()).Take(3).ToList();

            // Charger les informations complètes des livres sélectionnés
            var livres = await context.Livres
                .Include(l => l.Auteur)
                .Include(l => l.Exemplaires)
                .Where(l => randomIds.Contains(l.Id))
                .ToListAsync();

            // Créer les objets d'affichage pour chaque livre
            foreach (var l in livres)
            {
                LivresDecouverte.Add(new LivreAffichage
                {
                    Titre = l.Titre,
                    Auteur = l.Auteur.NomComplet,
                    // Sélectionner une description aléatoire parmi les prédéfinies
                    Description = LivreAffichage.Descriptions[_random.Next(LivreAffichage.Descriptions.Length)],
                    ImageUrl = "book.png"
                });
            }

            // Mettre à jour l'interface utilisateur sur le thread principal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                BindableLayout.SetItemsSource(LivresStack, LivresDecouverte);
            });
        }
        catch (Exception ex)
        {
            // Journaliser l'erreur (en production, utiliser un système de logging)
            Console.WriteLine($"Erreur lors du chargement des livres: {ex.Message}");
        }
    }
}