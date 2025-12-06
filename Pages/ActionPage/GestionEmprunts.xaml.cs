using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using BibliothequeManager.Services;
using BibliothequeManager.Views;
using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;
using static BibliothequeManager.Models.Reservation;

namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionEmprunts : ContentPage
{
    private readonly SessionUser session;
    public ICommand VoirCommand { get; }
    public ICommand RetournerCommand { get; }

    public GestionEmprunts(SessionUser user)
	{
		InitializeComponent();
        session = user;
        if(!session.EstConnecte)
        {
            Application.Current.MainPage = new NavigationPage(new Connexion());
            return;
        }

        FilterPicker.ItemsSource = StatutOptions;

        VoirCommand = new Command<Emprunt>(OnVoir);
        RetournerCommand = new Command<Emprunt>(OnRetourner);
        SearchEntry.TextChanged += OnSearchBarTextChanged;
        FilterPicker.SelectedIndexChanged += OnFilterPickerSelectedIndexChanged;

        BindingContext = this;
        ChargerEmprunts();
        ChargerStatistique();
    }

    private async void OnVoir(Emprunt emprunt)
    {
        if (emprunt != null)
        {

            var livre = $" Titre du livre : {emprunt.Exemplaire.Livre.Titre}";
            var adherent = $" Adhérent : {emprunt.Adherent.Prenom} {emprunt.Adherent.Nom}";
            var dates = $" Date d'emprunt : {emprunt.DateEmprunt:dd/MM/yyyy} \n Date de retour prévu : {emprunt.DateRetourPrevu:dd/MM/yyyy}";
            var statut = $" Statut : {emprunt.StatutEmprunt}";
            var amande = $" Amende : {emprunt.Amande}";
            await DetailPopup.Show(livre, adherent, dates, statut, amande, this);

        }
    }

    private async void OnRetourner(Emprunt emprunt)
    {

        if(emprunt != null)
        {
            using var context = new BibliothequeContext();
            var empruntDb = context.Emprunts.First(e => e.Id == emprunt.Id);
            empruntDb.DateRetourReel = DateTime.UtcNow;
            empruntDb.MettreAJourStatut();
            context.SaveChanges();
            await SuccessPopup.Show("Emprunt retournee avec succes.", this);
            ChargerEmprunts();
           
        }

    }

    private void ChargerEmprunts()
    {
        using var contexte = new BibliothequeContext();
        var emprunts = contexte.Emprunts
            .Include(e => e.Adherent)
            .Include(e => e.Exemplaire)
                .ThenInclude(ex => ex.Livre)
            .ToList();

        foreach (var e in emprunts)
        {
            e.MettreAJourStatut();
        }

        EmpruntsCollectionView.ItemsSource = emprunts;
    }

    private void ChargerStatistique()
    {
        using var contexte = new BibliothequeContext();
        var enCours = contexte.Emprunts.Count(e => e.StatutEmprunt == "En cours");
        var enRetard = contexte.Emprunts.Count(e => e.StatutEmprunt == "En retard");
        var retournee = contexte.Emprunts.Count(e => e.StatutEmprunt == "Retourné");
        var totalActif = enCours + enRetard + retournee;

        EmpruntsEnCours.Text = enCours.ToString();
        EmpruntsEnRetard.Text = enRetard.ToString();
        EmpruntsRetournes.Text = retournee.ToString();
        TotalEmprunt.Text = totalActif.ToString();
    }
    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    public List<string> StatutOptions { get; } = new()
    {
        App.Localized["All"],
        App.Localized["Pending"],
        App.Localized["InProgress"],
        App.Localized["Returned"]
    };

    private async void FiltrerEmprunts(string searchText)
    {
        using var context = new BibliothequeContext();
        try
        {
            var query = context.Emprunts
                .Include(l => l.Adherent)
                .Include(l => l.Exemplaire)
                .ThenInclude(ex => ex.Livre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(l =>
                    l.Exemplaire.Livre.Titre.Contains(searchText) ||
                    l.Exemplaire.Livre.ISBN.Contains(searchText) ||
                    (l.Adherent != null && l.Adherent.Nom.Contains(searchText)) ||
                    (l.Adherent != null && l.Adherent.Prenom.Contains(searchText)));
            }

            var emprunts = await query
                .OrderBy(l => l.Exemplaire.Livre.Titre)
                .AsNoTracking()
                .ToListAsync();

            EmpruntsCollectionView.ItemsSource = emprunts;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de filtrer : {ex.Message}", "OK");
        }
    }

    private void OnSearchBarTextChanged(object? sender, TextChangedEventArgs e)
    {
        FiltrerEmprunts(e.NewTextValue);
    }

    private void OnFilterPickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        using var context = new BibliothequeContext();

        string? statutFiltre = null;
        if (FilterPicker.SelectedIndex > 0)
        {
            var option = FilterPicker.SelectedItem?.ToString();
            statutFiltre = option switch
            {
                var s when s == App.Localized["InProgress"] => "En cours",
                var s when s == App.Localized["Late"] => "En retard",
                var s when s == App.Localized["Returned"] => "Retourné",
                _ => null
            };
        }

        var query = context.Emprunts
            .Include(e => e.Adherent)
            .Include(e => e.Exemplaire).ThenInclude(ex => ex.Livre)
            .AsQueryable();

        if (!string.IsNullOrEmpty(statutFiltre))
        {
            query = query.Where(e => e.StatutEmprunt == statutFiltre);
        }

        var emprunts = query.ToList();
        EmpruntsCollectionView.ItemsSource = emprunts;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ChargerEmprunts(); 
        ChargerStatistique(); 
    }
}