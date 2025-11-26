namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionEmprunts : ContentPage
{
	public GestionEmprunts()
	{
		InitializeComponent();
	}

    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new HomePage());
    }
}