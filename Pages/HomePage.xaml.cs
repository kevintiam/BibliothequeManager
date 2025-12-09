using BibliothequeManager.Models;
using BibliothequeManager.Pages.Views;
using BibliothequeManager.Services;
using BibliothequeManager.Views;
using System.Globalization;

namespace BibliothequeManager.Pages;

public partial class HomePage : FlyoutPage
{
	private readonly SessionUser session;
    public HomePage(SessionUser bibliothecaire)
    {
        InitializeComponent();
        session = bibliothecaire;

        if (!session.EstConnecte)
        {
            Application.Current.MainPage = new NavigationPage(new Connexion());
            return;
        }

        InfosUser(session.UtilisateurActuel!);
        LoadHomePage();
        NavigatePage();
    }

    private void LoadHomePage()
	{
		Detail = new NavigationPage(new Accueil(session));
		IsPresented = false;

    }
	private void NavigatePage()
	{
        AccueilButton.Clicked += (s, e) =>
		{
			Detail = new NavigationPage(new Accueil(session));
			IsPresented = false;
		};
        LivresButton.Clicked += (s, e) =>
		{
			Detail = new NavigationPage(new Books(session));
			IsPresented = false;
		};
		AuthorsButton.Clicked += (s, e) =>
		{
			Detail = new NavigationPage(new Authors(session));
			IsPresented = false;
		};
		CategorieButton.Clicked += (s, e) =>
		{
			Detail = new NavigationPage(new CategoriePage(session));
			IsPresented = false;
		};
		AdherentsButton.Clicked += (s, e) =>
		{
			Detail = new NavigationPage(new GestionAdherent(session));
			IsPresented = false;
		};
    }

    // Gestion du changement de langue
    private void OnSwitchLanguageClicked(object sender, EventArgs e)
    {
        string langueActuelle = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        string nouvelleLangue = langueActuelle == "fr" ? "en" : "fr";
        var culture = new CultureInfo(nouvelleLangue);

        // Applique cette culture partout dans l'application
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        // Recharge la page : on la retire, puis on la remet
        App.Localized.OnCultureChanged();

        //Detail = new NavigationPage(new Accueil(session));
        IsPresented = false;
    }

    private async void btnSingOut_Clicked(object sender, EventArgs e)
    {
		session.SeDeconnecter();
        Application.Current.MainPage = new NavigationPage(new Connexion());
    }

	public void InfosUser(Bibliothecaire user)
	{
		UserNameLabel.Text = user.Prenom;
		UserEmailLabel.Text = user.Email;
	}
}