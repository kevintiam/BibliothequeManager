namespace BibliothequeManager.Views;

public partial class Connexion : ContentPage
{
    public Connexion()
    {
        InitializeComponent();

        BindingContext = new ConnexionViewModel();
    }
}