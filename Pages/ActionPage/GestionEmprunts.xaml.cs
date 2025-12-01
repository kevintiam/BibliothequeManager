namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionEmprunts : ContentPage
{
	public GestionEmprunts()
	{
		InitializeComponent();
        FilterPicker.ItemsSource = StatutOptions;

    }

    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage());
    }

    public List<string> StatutOptions { get; } = new()
    {
        App.Localized["All"],
        App.Localized["InProgress"],
        App.Localized["Late"],
        App.Localized["Returned"]
    };
}