namespace BibliothequeManager.Pages.Views;

public partial class Authors : ContentPage
{
	public Authors()
	{
		InitializeComponent();
	}
    private void OnFloatingAddClicked(object sender, EventArgs e)
    {
        FormulaireLivres.IsVisible = !FormulaireLivres.IsVisible;
    }
}