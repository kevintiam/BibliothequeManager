using BibliothequeManager.Pages.Views;
using Microsoft.Maui.Controls;

namespace BibliothequeManager.Pages.ActionPage
{
    public partial class Reserver : ContentPage
    {
        public Reserver()
        {
            InitializeComponent();

            // Définir les dates par défaut
            DateDebutPicker.Date = DateTime.Now;
            DateFinPicker.Date = DateTime.Now.AddDays(7);
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

        private async void OnConfirmerClicked(object sender, EventArgs e)
        {
            // Validation
            if (!LivreSection.IsVisible)
            {
                await DisplayAlert("Erreur", "Veuillez d'abord sélectionner un livre", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(AbonneIdEntry.Text))
            {
                await DisplayAlert("Erreur", "Veuillez saisir l'ID de l'abonné", "OK");
                return;
            }

            if (DateFinPicker.Date <= DateDebutPicker.Date)
            {
                await DisplayAlert("Erreur", "La date de fin doit être après la date de début", "OK");
                return;
            }

            // Logique de réservation
            await DisplayAlert("Succès", "Réservation confirmée avec succès!", "OK");

            // Réinitialiser le formulaire
            LivreSection.IsVisible = false;
            RechercheEntry.Text = string.Empty;
            AbonneIdEntry.Text = string.Empty;
        }

        private async void OnAccueilClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnMesReservationsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GestionReservations());
        }
    }
}