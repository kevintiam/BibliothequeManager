namespace BibliothequeManager.Pages.Popups;

public partial class DetailPopup : ContentPage
{
    public event Action<bool> OnCompleted;

    public string Livre
    {
        get => LivreLabel.Text;
        set => LivreLabel.Text = value;
    }

    public string Adherent
    {
        get => AdherentLabel.Text;
        set => AdherentLabel.Text = value;
    }

    public string Dates
    {
        get => DatesLabel.Text;
        set => DatesLabel.Text = value;
    }
    public string Statut
    {
        get => StatutLabel.Text;
        set => StatutLabel.Text = value;
    }
    public string Amande
    {
        get => AmandeLabel.Text;
        set => AmandeLabel.Text = value;
    }

    public DetailPopup (string livre,string adherent,string dates,string statut,string amande)
	{
		InitializeComponent();
		Livre = livre;
		Adherent = adherent;
		Dates = dates;
		Statut = statut;
		Amande = amande;
	}
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        Navigation.PopModalAsync();
        return true;

    }

    public static async Task Show(string livre, string adherent, string dates, string statut, string amande, Page parentPage)
    {
        var popup = new DetailPopup(livre, adherent, dates, statut, amande);
        await parentPage.Navigation.PushModalAsync(popup);
    }

}