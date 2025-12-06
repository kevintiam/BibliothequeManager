using BibliothequeManager.Services;

namespace BibliothequeManager.Views;

public partial class Connexion : ContentPage
{
    //private readonly SessionUser session;
    public Connexion()
    {
        InitializeComponent();
        var services = MauiProgram.CreateMauiApp().Services;
        var session = services.GetRequiredService<SessionUser>();
        BindingContext = new ConnexionViewModel(session);
    }
}