using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using BibliothequeManager.Services;
using BibliothequeManager.Views;
using CommunityToolkit.Maui.Views;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI;
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

    /// <summary>
    /// Fonction pour afficher les détails d'un emprunt dans une popup
    /// </summary>
    /// <param name="emprunt"></param>
    private async void OnVoir(Emprunt emprunt)
    {
        if (emprunt != null)
        {

            var livre = $" Titre du livre : {emprunt.Exemplaire?.Livre?.Titre}";
            var adherent = $" Adhérent : {emprunt.Adherent?.Prenom} {emprunt.Adherent?.Nom}";
            var dates = $" Date d'emprunt : {emprunt.DateEmprunt:dd/MM/yyyy} \n Date de retour prévu : {emprunt.DateRetourPrevu:dd/MM/yyyy}";
            var statut = $" Statut : {emprunt.StatutEmprunt}";
            var amande = $" Amende : {emprunt.Amande:C}";
            await DetailPopup.Show(livre, adherent, dates, statut, amande, this);

        }
    }
    /// <summary>
    /// Fonction pour retourner un emprunt
    /// </summary>
    /// <param name="emprunt"></param>
    private async void OnRetourner(Emprunt emprunt)
    {
        if(emprunt != null)
        {
            using var context = new BibliothequeContext();
            try
            {
                var popup = new ConfirmationPopup
                {
                    Title = App.Localized["ConfirmDelete"],
                    Message = $"Confirmez-vous le retour de \"{emprunt.Exemplaire?.Livre?.Titre}\" ?"
                };

                popup.OnCompleted += async (confirmed) =>
                {
                    if (confirmed)
                    {
                        var empruntDb = await context.Emprunts.FirstAsync(e => e.Id == emprunt.Id);
                        empruntDb.DateRetourReel = DateTime.UtcNow;
                        empruntDb.MettreAJourStatut();
                        empruntDb.BibliothecaireRetourId = session.UtilisateurActuel?.Id;
                        var exemplaire = await context.Exemplaires.FindAsync(empruntDb.ExemplaireId);
                        if (exemplaire != null)
                        {
                            exemplaire.EstDisponible = true;
                        }
                        await context.SaveChangesAsync();
                        await SuccessPopup.Show("Emprunt retourné avec succès.", this);
                        await ChargerEmprunts();
                        await ChargerStatistique();
                    }
                };
                await Navigation.PushModalAsync(popup);
            }
            catch (Exception ex)
            {
                await ErrorPopup.Show($"Erreur lors de la confirmation : {ex.Message}", this);
            }
        }
    }
    /// <summary>
    /// Fonction pour charger les emprunts depuis la base de données
    /// </summary>
    private async Task ChargerEmprunts()
    {
        using var contexte = new BibliothequeContext();
        var emprunts = await contexte.Emprunts
            .Include(e => e.Adherent)
            .Include(e => e.Exemplaire)
                .ThenInclude(ex => ex.Livre)
            .ToListAsync();

        foreach (var e in emprunts)
        {
            e.MettreAJourStatut();
        }

        EmpruntsCollectionView.ItemsSource = emprunts;
    }
    /// <summary>
    /// Fonction pour charger les statistiques des emprunts
    /// </summary>
    private async Task ChargerStatistique()
    {
        using var contexte = new BibliothequeContext();
        var enCours = await contexte.Emprunts.CountAsync(e => e.StatutEmprunt == "En cours");
        var enRetard = await contexte.Emprunts.CountAsync(e => e.StatutEmprunt == "En retard");
        var retournee = await contexte.Emprunts.CountAsync(e => e.StatutEmprunt == "Retourné");
        var totalActif = enCours + enRetard + retournee;

        EmpruntsEnCours.Text = enCours.ToString();
        EmpruntsEnRetard.Text = enRetard.ToString();
        EmpruntsRetournes.Text = retournee.ToString();
        TotalEmprunt.Text = totalActif.ToString();
    }
    /// <summary>
    /// Bouton pour retourner à la page d'accueil
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    /// <summary>
    /// Liste des options de statut pour le filtre
    /// </summary>
    public List<string> StatutOptions { get; } = new()
    {
        App.Localized["All"],
        App.Localized["Pending"],
        App.Localized["InProgress"],
        App.Localized["Returned"]
    };
    /// <summary>
    /// Fonction pour filtrer les emprunts en fonction du texte de recherche
    /// </summary>
    /// <param name="searchText"></param>
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
            await ErrorPopup.Show($"Impossible de filtrer : {ex.Message}", this);
        }
    }
    /// <summary>
    /// Fonction déclenchée lors du changement de texte dans la barre de recherche
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnSearchBarTextChanged(object? sender, TextChangedEventArgs e)
    {
        FiltrerEmprunts(e.NewTextValue);
    }
    /// <summary>
    /// Fonction déclenchée lors du changement de sélection dans le picker de filtre
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
    /// <summary>
    /// Fonction déclenchée lors de l'apparition de la page
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ChargerEmprunts(); 
        await ChargerStatistique(); 
    }
}