using BibliothequeManager.Pages.Views;

namespace BibliothequeManager.Pages.ActionPage;

public partial class Retour : ContentPage
{
    public Retour()
    {
        InitializeComponent();
    }
    private async void OnConfirmerClicked(object sender, System.EventArgs e)
    {

        if (string.IsNullOrWhiteSpace(AbonneIdEntry.Text))
        {
            await DisplayAlert("Erreur", "Veuillez entrer l'ID de l'abonné.", "OK");
            return;
        }

        // Récupération des données
        var abonneId = AbonneIdEntry.Text;


        // Ici, vous pouvez ajouter la logique pour enregistrer l'emprunt dans votre base de données
        // Par exemple : 
        // await _empruntService.AjouterEmpruntAsync(isbn, abonneId, dateRetour);


        // Réinitialiser le formulaire
        AbonneIdEntry.Text = string.Empty;

    }

    private async void OnAccueilClicked(object sender, System.EventArgs e)
    {
        // Navigation vers la page d'accueil
        await Navigation.PopAsync();
    }

    private void OnFloatingAddAdherentClicked(object sender, EventArgs e)
    {
        ListeEmprunts.IsVisible = !ListeEmprunts.IsVisible;
        RetourContents.IsVisible = !RetourContents.IsVisible;
        HeaderText.IsVisible = !HeaderText.IsVisible;

    }

}