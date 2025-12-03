using BibliothequeManager.Pages.Views;
using System.Globalization;

namespace BibliothequeManager.Pages;

public partial class HomePage : FlyoutPage
{
	public HomePage()
	{
        InitializeComponent();
		LoadHomePage();
		NavigatePage();
    }

	private void LoadHomePage()
	{
		Detail = new Accueil();
		IsPresented = false;

    }
	private void NavigatePage()
	{
        AccueilButton.Clicked += (s, e) =>
		{
			Detail = new Accueil();
			IsPresented = false;
		};
        LivresButton.Clicked += (s, e) =>
		{
			Detail = new Books();
			IsPresented = false;
		};
		AuthorsButton.Clicked += (s, e) =>
		{
			Detail = new Authors();
			IsPresented = false;
		};
		CategorieButton.Clicked += (s, e) =>
		{
			Detail = new CategoriePage();
			IsPresented = false;
		};
		AdherentsButton.Clicked += (s, e) =>
		{
			Detail = new Adherent();
			IsPresented = false;
		};


    }


    private void OnSwitchLanguageClicked(object sender, EventArgs e)
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
}