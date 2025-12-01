namespace BibliothequeManager.Pages.Popups;

public partial class ConfirmationPopup : ContentPage
{
    public event Action<bool> OnCompleted;

    public string Title
    {
        get => TitleLabel.Text;
        set => TitleLabel.Text = value;
    }

    public string Message
    {
        get => MessageLabel.Text;
        set => MessageLabel.Text = value;
    }

    public ConfirmationPopup()
    {
        InitializeComponent();
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        OnCompleted?.Invoke(true);
        await Navigation.PopModalAsync(); 
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        OnCompleted?.Invoke(false);
        await Navigation.PopModalAsync();
    }

}