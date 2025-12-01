using BibliothequeManager.Pages.Views;

namespace BibliothequeManager.Pages.ActionPage;

public partial class Emprunt : ContentPage
{
	public Emprunt()
	{
		InitializeComponent();
	}
    private async void OnConfirmerClicked(object sender, System.EventArgs e)
    {
        // Logique de confirmation
    }

    private async void OnRechercherClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RechercheEntry.Text))
        {
            await DisplayAlert("Recherche", "Veuillez saisir un titre, auteur ou ISBN", "OK");
            return;
        }

        // Simulation de recherche
        LivreSection.IsVisible = true;
        TitreLivre.Text = "Les Misérables";
        AuteurLivre.Text = "Victor Hugo";
        IsbnLivre.Text = "978-2-07-040813-8";
        DisponibiliteLivre.Text = "Disponible";
    }
    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        // Navigation vers la page d'accueil
        await Navigation.PopAsync();
    }

    private async void OnListeEmpruntsClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new GestionEmprunts());

    }
}
