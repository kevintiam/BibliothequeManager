namespace BibliothequeManager.Pages.Views;

public partial class Books : ContentPage
{
	public Books()
	{
		InitializeComponent();
	}
    private void OnFloatingAddClicked(object sender, EventArgs e)
    {
        FormulaireLivres.IsVisible = !FormulaireLivres.IsVisible;
    }
}