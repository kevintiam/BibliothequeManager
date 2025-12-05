using BibliothequeManager.Models;
using BibliothequeManager.Pages.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BibliothequeManager.Pages.ActionPage;

public partial class Retour : ContentPage
{
    private readonly BibliothequeContext contexte = new();
    private Adherent adherentCourant;
    private List<Emprunt> empruntsActifs;

    // Classe pour afficher les emprunts dans la liste
    public class EmpruntAffiche
    {
        public string TitreLivre { get; set; } = "";
        public string Auteur { get; set; } = "";
        public string ISBN { get; set; } = "";
        public DateTime DateEmprunt { get; set; }
        public DateTime DateRetour { get; set; }
        public int IdEmprunt { get; set; }
    }

    public Retour()
    {
        InitializeComponent();
    }

    // → SEULE VERSION DE OnConfirmerClicked
    private async void OnConfirmerClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AbonneIdEntry.Text))
        {
            await DisplayAlert("Erreur", "Veuillez entrer l'ID de l'abonné.", "OK");
            return;
        }

        if (!int.TryParse(AbonneIdEntry.Text, out int idAdherent))
        {
            await DisplayAlert("Erreur", "ID invalide.", "OK");
            return;
        }

        try
        {
            Adherent adherent = await contexte.Adherents
                .Include(a => a.Emprunts)
                    .ThenInclude(e => e.Exemplaire)
                        .ThenInclude(ex => ex.Livre)
                        .ThenInclude(l => l.Auteur)
                .FirstOrDefaultAsync(a => a.Id == idAdherent);

            if (adherent == null)
            {
                await DisplayAlert("Erreur", "Adhérent non trouvé.", "OK");
                return;
            }

            List<Emprunt> listeEmprunts = adherent.Emprunts
                .Where(e => !e.DateRetourReel.HasValue)
                .ToList();

            if (listeEmprunts.Count == 0)
            {
                await DisplayAlert("Info", "Cet adhérent n'a aucun emprunt en cours.", "OK");
                return;
            }

            adherentCourant = adherent;
            empruntsActifs = listeEmprunts;

            NomAbonne.Text = adherent.Nom;
            PrenomAbonne.Text = adherent.Prenom;
            IDAbonne.Text = $"ID : {adherent.Id}";
            NbLivreEmprunter.Text = $"Livres empruntés : {listeEmprunts.Count}";

            List<EmpruntAffiche> empruntsAAfficher = new();
            foreach (Emprunt emprunt in listeEmprunts)
            {
                empruntsAAfficher.Add(new EmpruntAffiche
                {
                    TitreLivre = emprunt.Exemplaire?.Livre?.Titre ?? "Titre inconnu",
                    Auteur = emprunt.Exemplaire?.Livre?.Auteur?.Nom ?? "Auteur inconnu",
                    ISBN = emprunt.Exemplaire?.Livre?.ISBN ?? "ISBN inconnu",
                    DateEmprunt = emprunt.DateEmprunt.ToLocalTime().Date,
                    DateRetour = emprunt.DateRetourPrevu.ToLocalTime().Date,
                    IdEmprunt = emprunt.Id
                });
            }

            EmpruntsCollectionView.ItemsSource = empruntsAAfficher;

            ListeEmprunts.IsVisible = true;
            RetourContents.IsVisible = false;
            HeaderText.IsVisible = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de charger les données : {ex.Message}", "OK");
        }
    }

    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnFloatingAddAdherentClicked(object sender, EventArgs e)
    {
        ListeEmprunts.IsVisible = !ListeEmprunts.IsVisible;
        RetourContents.IsVisible = !RetourContents.IsVisible;
        HeaderText.IsVisible = !HeaderText.IsVisible;
    }
}