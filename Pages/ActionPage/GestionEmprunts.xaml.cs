using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionEmprunts : ContentPage
{
    public ICommand VoirCommand { get; }
    public ICommand RetournerCommand { get; }

    public GestionEmprunts()
	{
		InitializeComponent();
        FilterPicker.ItemsSource = StatutOptions;

        VoirCommand = new Command<Emprunt>(OnVoir);
        RetournerCommand = new Command<Emprunt>(OnRetourner);

        BindingContext = this;
        ChargerEmprunts();
        ChargerStatistique();

    }

    private async void OnVoir(Emprunt emprunt)
    {
      if (emprunt == null) 
        {
            await ErrorPopup.Show("Aucun emprunt sélectionné.", this);
             return;
       }
        var livre = $"Livre Emprunté : {emprunt.Exemplaire?.Livre?.Titre ?? "N/A"}";
        if (livre == null) {
            livre = "N/A";
        }
        var adherent = $"Adhérent : {emprunt.Adherent?.NomComplet ?? "N/A"}";
        if (adherent == null) {
            adherent = "N/A";
        }
        var dates = $"Date Emprunt : {emprunt.DateEmprunt:dd/MM/yy} \n Date de Retour prévu : {emprunt.DateRetourPrevu:dd/MM/yy}";
        var statut = $"Statut : {emprunt.StatutEmprunt}";
        var amande = $"Amende : {emprunt.Amande}";
        await DetailPopup.Show(livre, adherent, dates, statut, amande, this);
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
            empruntDb.MettreAJourStatut();
            context.SaveChanges();

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