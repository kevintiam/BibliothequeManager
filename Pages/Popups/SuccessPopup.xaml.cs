namespace BibliothequeManager.Pages.Popups;

public partial class SuccessPopup : ContentPage
{
    private readonly CancellationTokenSource cts = new();

    public string Message
    {
        get => MessageLabel.Text;
        set => MessageLabel.Text = value;
    }

    public SuccessPopup(string message = "Opération réussie !")
    {
        InitializeComponent();
        Message = message;
        CloseAfterDelay();
    }

    private async void CloseAfterDelay()
    {
        try
        {
            await Task.Delay(5000, cts.Token);
            await CloseAsync();
        }
        catch (TaskCanceledException)
        {
            // Annulé (clic utilisateur)
        }
    }

    private async Task CloseAsync()
    {
        cts.Cancel(); 
        await Navigation.PopModalAsync();
    }

    public static async Task Show(string message, Page parentPage)
    {
        var popup = new SuccessPopup(message);
        await parentPage.Navigation.PushModalAsync(popup);
    }
}