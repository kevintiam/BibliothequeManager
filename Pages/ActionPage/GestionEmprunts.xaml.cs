using BibliothequeManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionEmprunts : ContentPage
{
    public ICommand RetournerCommand { get; }

    public GestionEmprunts()
    {
        InitializeComponent();
        RetournerCommand = new Command<Emprunt>(OnRetourner);
        BindingContext = this;
        ChargerEmprunts();
        ChargerStatistique(); // ← facultatif, voir note ci-dessous
    }

    private async void OnRetourner(Emprunt emprunt)
    {
        if (emprunt.DateRetourReel.HasValue)
        {
            await DisplayAlert("Info", "Cet emprunt est déjà retourné.", "OK");
            return;
        }

        string titre = emprunt.Exemplaire?.Livre?.Titre ?? "ce livre";
        bool confirmer = await DisplayAlert("Confirmer", $"Retourner «{titre}» ?", "Oui", "Annuler");
        if (!confirmer) return;

        try
        {
            using var context = new BibliothequeContext();
            var empruntDb = context.Emprunts
                .Include(e => e.Exemplaire)
                .First(e => e.Id == emprunt.Id);

            empruntDb.DateRetourReel = DateTime.UtcNow;
            empruntDb.BibliothecaireRetourId = null; // ou App.UtilisateurConnecte?.Id plus tard
            empruntDb.MettreAJourStatut();

            if (empruntDb.Exemplaire != null)
            {
                empruntDb.Exemplaire.EstDisponible = true;
            }

            context.SaveChanges();
            await DisplayAlert("Succès", $"«{titre}» a été retourné.", "OK");

            ChargerEmprunts();
            ChargerStatistique(); // ← facultatif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Échec du retour : {ex.Message}", "OK");
        }
    }

    private void ChargerEmprunts()
    {
        using var contexte = new BibliothequeContext();
        var emprunts = contexte.Emprunts
            .Include(e => e.Adherent)
            .Include(e => e.Exemplaire)
                .ThenInclude(ex => ex.Livre)
            .Where(e => !e.DateRetourReel.HasValue) // ← SEULEMENT les emprunts actifs
            .ToList();

        foreach (var e in emprunts)
        {
            e.MettreAJourStatut(); // met à jour "En cours" / "En retard"
        }

        EmpruntsCollectionView.ItemsSource = emprunts;
    }

    private void ChargerStatistique()
    {
        using var contexte = new BibliothequeContext();
        var enCours = contexte.Emprunts.Count(e => e.StatutEmprunt == "En cours");
        var enRetard = contexte.Emprunts.Count(e => e.StatutEmprunt == "En retard");
        var totalActif = enCours + enRetard;

        // Mettez à jour vos Labels (à adapter selon vos noms réels)
        EmpruntsEnCours.Text = enCours.ToString();
        EmpruntsEnRetard.Text = enRetard.ToString();
        TotalEmprunt.Text = totalActif.ToString();
        // → Supprimez cette méthode si vous n’affichez pas ces stats
    }

    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}