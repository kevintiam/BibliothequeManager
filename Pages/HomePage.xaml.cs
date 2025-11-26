using BibliothequeManager.Pages.Views;

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
			Detail = new Categorie();
			IsPresented = false;
		};
		AdherentsButton.Clicked += (s, e) =>
		{
			Detail = new Adherent();
			IsPresented = false;
		};


    }
}