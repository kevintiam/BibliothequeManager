namespace BibliothequeManager.Pages.Popups;

public partial class ErrorPopup : ContentPage
{
    public string Message
    {
        get => MessageLabel.Text;
        set => MessageLabel.Text = value;
    }

    public ErrorPopup(string message = "Une erreur est survenue.")
    {
        InitializeComponent();
        Message = message;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    //  permettre la fermeture avec la touche "Retour" (Android)
    protected override bool OnBackButtonPressed()
    {
        Navigation.PopModalAsync();
        return true;

    }

    public static async Task Show(string message, Page parentPage)
    {
        var popup = new ErrorPopup(message);
        await parentPage.Navigation.PushModalAsync(popup);
    }
}