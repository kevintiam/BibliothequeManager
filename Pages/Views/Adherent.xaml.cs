namespace BibliothequeManager.Pages.Views;

public partial class Adherent : ContentPage
{
	public Adherent()
	{
		InitializeComponent();
	}
    private void OnFloatingAddAdherentClicked(object sender, EventArgs e)
    {
        FormulaireAdherent.IsVisible = !FormulaireAdherent.IsVisible;
    }
}