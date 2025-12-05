using BibliothequeManager.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionEmprunts : ContentPage
{
    public ICommand VoirCommand { get; }
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
        ChargerStatistique();

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

    private void OnRetourner(Emprunt emprunt)
    {
        if (emprunt.DateRetourReel.HasValue)
        {
            DisplayAlert("Déjà rendu", "Cet emprunt est déjà enregistré comme rendu.", "OK");
            return;
        }

            using var context = new BibliothequeContext();
            var empruntDb = context.Emprunts.First(e => e.Id == emprunt.Id);
            empruntDb.DateRetourReel = DateTime.UtcNow;
            empruntDb.MettreAJourStatut(); // met à jour StatutEmprunt = "Retourné"
            context.SaveChanges();

            // Rafraîchir la liste pour refléter le changement
            ChargerEmprunts();
        
    }
    private void ChargerEmprunts()
    {
        using var donnee = new BibliothequeContext();
        var emprunts = donnee.Emprunts
            .Include(e => e.Adherent)
            .Include(e => e.Exemplaire)
                .ThenInclude(ex => ex.Livre)
            .ToList();

        foreach (var e in emprunts)
        {
            e.MettreAJourStatut();
        }

        donnee.SaveChanges();
        ChargerStatistique();
        EmpruntsCollectionView.ItemsSource = emprunts;
    }

    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage());
    }

    private void ChargerStatistique()
    {
        using var donnee = new BibliothequeContext();
        var nbStatutEnCours = donnee.Emprunts.Count(e => e.StatutEmprunt == "En cours");
        var nbRetarde = donnee.Emprunts.Count(e => e.StatutEmprunt == "En retard");
        var nbStatutRetournee = donnee.Emprunts.Count(e => e.StatutEmprunt == "Retourné");
        var total = donnee.Emprunts.Count();

        TotalEmprunt.Text = total.ToString();
        EmpruntsEnCours.Text = nbStatutEnCours.ToString();
        EmpruntsEnRetard.Text = nbRetarde.ToString();
        EmpruntsRetournes.Text = nbStatutRetournee.ToString();

    }

    public List<string> StatutOptions { get; } = new()
    {
        App.Localized["All"],
        App.Localized["InProgress"],
        App.Localized["Late"],
        App.Localized["Returned"]
    };
}