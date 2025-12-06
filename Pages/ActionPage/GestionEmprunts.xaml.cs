using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionEmprunts : ContentPage
{
    public ICommand VoirCommand { get; }
    public ICommand RelancerCommand { get; }
    public ICommand RelancerCommand { get; }
    public ICommand RetournerCommand { get; }

    public GestionEmprunts()
	{
		InitializeComponent();
        FilterPicker.ItemsSource = StatutOptions;

        VoirCommand = new Command<Emprunt>(OnVoir);
        RelancerCommand = new Command<Emprunt>(OnRelancer);
        RetournerCommand = new Command<Emprunt>(OnRetourner);


        BindingContext = this;
        ChargerEmprunts();
        ChargerStatistique(); // ← facultatif, voir note ci-dessous
    }

    private void OnVoir(Emprunt emprunt)
    {
        // Ex: naviguer vers une page de détails
        // await Shell.Current.GoToAsync($"//DetailEmpruntPage?id={emprunt.Id}");
        DisplayAlert("Détails", $"Emprunt de {emprunt.Adherent?.NomComplet} - {emprunt.Exemplaire?.Livre?.Titre}", "OK");
    }

    private void OnRelancer(Emprunt emprunt)
    {
        if (emprunt.JoursRestants >= 0)
        {
            DisplayAlert("Info", "Pas de relance nécessaire – pas en retard.", "OK");
            return;
        }

        // Simulation d’envoi d’email ou notification
        DisplayAlert("Relance", $"Un rappel a été envoyé à {emprunt.Adherent?.Email ?? "l'adhérent"} pour retard de {Math.Abs(emprunt.JoursRestants)} j.", "OK");
    }

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

            using var context = new BibliothequeContext();
            var empruntDb = context.Emprunts.First(e => e.Id == emprunt.Id);
            empruntDb.DateRetourReel = DateTime.UtcNow;
            empruntDb.MettreAJourStatut(); // met à jour StatutEmprunt = "Retourné"
            context.SaveChanges();
            await DisplayAlert("Succès", $"«{titre}» a été retourné.", "OK");

            ChargerEmprunts();
        
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