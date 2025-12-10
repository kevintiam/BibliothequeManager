using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using BibliothequeManager.Pages.Views;
using BibliothequeManager.Services;
using BibliothequeManager.Views;
using System.Globalization;

namespace BibliothequeManager.Pages;

public partial class HomePage : FlyoutPage
{
    // Session utilisateur pour gérer l'authentification et l'état de l'application
    private readonly SessionUser session;

    /// <summary>
    /// Constructeur de la page principale avec menu latéral
    /// </summary>
    /// <param name="bibliothecaire">Session utilisateur (bibliothécaire) actuel</param>
    public HomePage(SessionUser bibliothecaire)
    {
        InitializeComponent();
        session = bibliothecaire;

        // Vérifier si l'utilisateur est connecté
        if (!session.EstConnecte)
        {
            // Rediriger vers la page de connexion si non connecté
            Application.Current.MainPage = new NavigationPage(new Connexion());
            return;
        }

        // Initialiser l'interface utilisateur
        InfosUser(session.UtilisateurActuel!);
        LoadHomePage();
        NavigatePage();
    }

    /// <summary>
    /// Charge la page d'accueil comme page principale au démarrage
    /// </summary>
    private void LoadHomePage()
    {
        // Définir la page d'accueil comme page de détail (contenu principal)
        Detail = new NavigationPage(new Accueil(session));

        // Fermer le menu latéral
        IsPresented = false;
    }

    /// <summary>
    /// Configure la navigation entre les différentes pages de l'application
    /// </summary>
    private void NavigatePage()
    {
        // Navigation vers la page d'accueil
        AccueilButton.Clicked += (s, e) =>
        {
            Detail = new NavigationPage(new Accueil(session));
            IsPresented = false; // Fermer le menu après sélection
        };

        // Navigation vers la page de gestion des livres
        LivresButton.Clicked += (s, e) =>
        {
            Detail = new NavigationPage(new Books(session));
            IsPresented = false;
        };

        // Navigation vers la page de gestion des auteurs
        AuthorsButton.Clicked += (s, e) =>
        {
            Detail = new NavigationPage(new Authors(session));
            IsPresented = false;
        };

        // Navigation vers la page de gestion des catégories
        CategorieButton.Clicked += (s, e) =>
        {
            Detail = new NavigationPage(new CategoriePage(session));
            IsPresented = false;
        };

        // Navigation vers la page de gestion des adhérents
        AdherentsButton.Clicked += (s, e) =>
        {
            Detail = new NavigationPage(new GestionAdherent(session));
            IsPresented = false;
        };
    }

    /// <summary>
    /// Gère le changement de langue de l'interface utilisateur
    /// </summary>
    private void OnSwitchLanguageClicked(object sender, EventArgs e)
    {
        // Déterminer la langue actuelle
        string langueActuelle = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        // Basculer entre français et anglais
        string nouvelleLangue = langueActuelle == "fr" ? "en" : "fr";
        var culture = new CultureInfo(nouvelleLangue);

        // Appliquer la nouvelle culture à tous les niveaux de l'application
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        // Notifier le changement de culture pour les ressources localisées
        App.Localized.OnCultureChanged();

        // Fermer le menu latéral après changement de langue
        IsPresented = false;

        // Note: Dans une application réelle, vous pourriez vouloir recharger 
        // la page courante pour appliquer les nouvelles traductions
        // Detail = new NavigationPage(new Accueil(session));
    }

    /// <summary>
    /// Gère la déconnexion de l'utilisateur
    /// </summary>
    private async void btnSingOut_Clicked(object sender, EventArgs e)
    {
        var popup = new ConfirmationPopup
        {
            Title = "Deconnexion",
            Message = "Êtes-vous sûr de vouloir vous déconnecter ?",
            Confirm = "Oui",
            Cancel = "Non"
        };

        popup.OnCompleted += async (confirmed) =>
        {
            if (confirmed)
            {
                // Déconnecter l'utilisateur
                session.SeDeconnecter();
                Application.Current.MainPage = new NavigationPage(new Connexion());
            }
        };

        await Navigation.PushModalAsync(popup);
    }

    /// <summary>
    /// Affiche les informations de l'utilisateur connecté dans le menu latéral
    /// </summary>
    /// <param name="user">Bibliothécaire connecté</param>
    public void InfosUser(Bibliothecaire user)
    {
        UserNameLabel.Text = $"{user.Prenom} {user.Nom}";
        UserEmailLabel.Text = user.Email;
    }

    /// <summary>
    /// Gère la disparition de la page (optionnel)
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}