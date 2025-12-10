using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using BibliothequeManager.Pages.Views;
using BibliothequeManager.Services;
using BibliothequeManager.Views;
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
        public DateTime DateRetourPrevu { get; set; }
        public DateTime? DateRetourReel { get; set; }
        public int IdEmprunt { get; set; }
        public bool EstEnRetard { get; set; }
        public decimal? Amende { get; set; }
    }

    private readonly SessionUser session;

    public Retour(SessionUser user)
    {
        InitializeComponent();
        session = user;

        if (!session.EstConnecte)
        {
            Application.Current.MainPage = new NavigationPage(new Connexion());
            return;
        }
    }

    /// <summary>
    /// Bouton pour confirmer l'ID de l'abonné et charger ses emprunts en cours
    /// </summary>
    private async void OnConfirmerClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AbonneIdEntry.Text))
        {
            await ErrorPopup.Show("Veuillez entrer l'ID de l'abonné.", this);
            return;
        }

        if (!int.TryParse(AbonneIdEntry.Text, out int idAdherent))
        {
            await ErrorPopup.Show("ID invalide.", this);
            return;
        }

        try
        {
            await ChargerEmpruntsAdherent(idAdherent);
        }
        catch (Exception ex)
        {
            await ErrorPopup.Show($"Impossible de charger les données : {ex.Message}", this);
        }
    }

    /// <summary>
    /// Charger les emprunts d'un adhérent
    /// </summary>
    private async Task ChargerEmpruntsAdherent(int idAdherent)
    {
        var adherent = await contexte.Adherents
            .Include(a => a.Emprunts)
                .ThenInclude(e => e.Exemplaire)
                    .ThenInclude(ex => ex.Livre)
                    .ThenInclude(l => l.Auteur)
            .FirstOrDefaultAsync(a => a.Id == idAdherent);

        if (adherent == null)
        {
            await ErrorPopup.Show("Adhérent non trouvé.", this);
            return;
        }

        List<Emprunt> listeEmprunts = adherent.Emprunts
            .Where(e => !e.DateRetourReel.HasValue)
            .OrderByDescending(e => e.DateEmprunt)
            .ToList();

        if (listeEmprunts.Count == 0)
        {
            await ErrorPopup.Show("Cet adhérent n'a aucun emprunt en cours.", this);
            return;
        }

        adherentCourant = adherent;
        empruntsActifs = listeEmprunts;

        // Mettre à jour les informations de l'adhérent
        NomAbonne.Text = adherent.Nom;
        PrenomAbonne.Text = adherent.Prenom;
        IDAbonne.Text = $"ID : {adherent.Id}";
        NbLivreEmprunter.Text = $"Livres empruntés : {listeEmprunts.Count}";
   
        // Préparer les données pour l'affichage
        var empruntsAAfficher = listeEmprunts.Select(async emprunt => new EmpruntAffiche
        {
            TitreLivre = emprunt.Exemplaire?.Livre?.Titre ?? "Titre inconnu",
            Auteur = emprunt.Exemplaire?.Livre?.Auteur?.Nom ?? "Auteur inconnu",
            ISBN = emprunt.Exemplaire?.Livre?.ISBN ?? "ISBN inconnu",
            DateEmprunt = emprunt.DateEmprunt.ToLocalTime(),
            DateRetourPrevu = emprunt.DateRetourPrevu.ToLocalTime(),
            IdEmprunt = emprunt.Id,
            EstEnRetard = emprunt.DateRetourPrevu < DateTime.UtcNow,
            Amende = CalculerAmende(emprunt.DateRetourPrevu, DateTime.UtcNow)
        }).ToList();


        EmpruntsCollectionView.ItemsSource = empruntsAAfficher;

        // Afficher la section des emprunts
        ListeEmprunts.IsVisible = true;
        RetourContents.IsVisible = false;
        HeaderText.IsVisible = false;
    }

    /// <summary>
    /// Retourner un emprunt sélectionné
    /// </summary>
    private async void OnRetournerClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is EmpruntAffiche empruntAffiche)
        {
            await RetournerEmprunt(empruntAffiche.IdEmprunt);
        }
    }

    /// <summary>
    /// Retourner tous les emprunts
    /// </summary>
    private async void OnRetournerToutClicked(object sender, EventArgs e)
    {
        if (empruntsActifs == null || !empruntsActifs.Any())
        {
            await ErrorPopup.Show("Aucun emprunt à retourner.", this);
            return;
        }

        // Demande de confirmation
        bool confirm = await DisplayAlert(
            "Retourner tous les livres",
            $"Confirmez-vous le retour des {empruntsActifs.Count} livre(s) emprunté(s) ?",
            "Oui", "Non");

        if (!confirm) return;

        try
        {
            // Retourner chaque emprunt
            foreach (var emprunt in empruntsActifs)
            {
                await RetournerEmpruntAvecTransaction(emprunt.Id, adherentCourant.Id);
            }

            await SuccessPopup.Show("Tous les livres ont été retournés avec succès.", this);

            // Réinitialiser l'affichage
            await ReinitialiserAffichage();
        }
        catch (Exception ex)
        {
            await ErrorPopup.Show($"Erreur lors du retour : {ex.Message}", this);
        }
    }

    /// <summary>
    /// Retourner un emprunt spécifique
    /// </summary>
    private async Task RetournerEmprunt(int empruntId)
    {
        try
        {
            // Demande de confirmation
            var emprunt = empruntsActifs.FirstOrDefault(e => e.Id == empruntId);
            if (emprunt == null) return;

            var titreLivre = emprunt.Exemplaire?.Livre?.Titre ?? "livre inconnu";

            var popup = new ConfirmationPopup
            {
                Title = App.Localized["ConfirmDelete"],
                Message = $"Confirmez-vous le retour de \"{titreLivre}\" ?"
            };

            popup.OnCompleted += async (confirmed) =>
            {
                if (!confirmed) return;

                // Effectuer le retour avec transaction
                var result = await RetournerEmpruntAvecTransaction(empruntId, adherentCourant.Id);

                if (result.Success)
                {
                    await SuccessPopup.Show($"Le livre \"{titreLivre}\" a été retourné avec succès.", this);

                    // Mettre à jour la liste
                    await ChargerEmpruntsAdherent(adherentCourant.Id);
                }
                else
                {
                    await ErrorPopup.Show(result.Message, this);
                }
            };

            await Navigation.PushModalAsync(popup); 
        }
        catch (Exception ex)
        {
            await ErrorPopup.Show($"Erreur : {ex.Message}", this);
        }
    }

    /// <summary>
    /// Retourner un emprunt avec transaction
    /// </summary>
    private async Task<(bool Success, string Message)> RetournerEmpruntAvecTransaction(int empruntId, int adherentId)
    {
        using var transaction = await contexte.Database.BeginTransactionAsync();

        try
        {
            // Charger l'emprunt avec ses relations
            var emprunt = await contexte.Emprunts
                .Include(e => e.Exemplaire)
                    .ThenInclude(ex => ex.Livre)
                .Include(e => e.Adherent)
                .FirstOrDefaultAsync(e => e.Id == empruntId);

            if (emprunt == null)
            {
                return (false, "Emprunt introuvable.");
            }

            // Vérifier si déjà retourné
            if (emprunt.DateRetourReel.HasValue)
            {
                return (false, "Ce livre a déjà été retourné.");
            }

            // Mettre à jour l'emprunt
            emprunt.DateRetourReel = DateTime.UtcNow;
            emprunt.MettreAJourStatut();

            var amande = CalculerAmende(emprunt.DateRetourPrevu, emprunt.DateRetourReel.Value);
            if (amande > 0)
            {
                emprunt.Adherent.Amandes += amande;
            }

            // Rendre l'exemplaire disponible
            if (emprunt.Exemplaire != null)
            {
                emprunt.Exemplaire.EstDisponible = true;
            }

            // Enregistrer les modifications
            await contexte.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, string.Empty);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return (false, "Les données ont été modifiées entre-temps. Veuillez réessayer.");
        }
        catch (DbUpdateException dbEx)
        {
            await transaction.RollbackAsync();
            return (false, $"Erreur de base de données : {dbEx.InnerException?.Message ?? dbEx.Message}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Erreur inattendue : {ex.Message}");
        }
    }

    /// <summary>
    /// Calculer l'amende pour retard
    /// </summary>
    private  decimal CalculerAmende(DateTime dateRetourPrevu, DateTime dateRetourReel)
    {
        const decimal tarifParJour = 0.50m; 
        const int joursGrace = 0; 
        const decimal amendeMax = 10.00m;

        var joursDeRetard = (dateRetourReel - dateRetourPrevu).Days;

        if (joursDeRetard > joursGrace)
        {
            var joursFactures = joursDeRetard - joursGrace;
            var amendeCalculee = joursFactures * tarifParJour;
            return Math.Min(amendeCalculee, amendeMax);
        }
        return 0;
    }

    /// <summary>
    /// Réinitialiser l'affichage
    /// </summary>
    private async Task ReinitialiserAffichage()
    {
        // Réinitialiser les champs
        AbonneIdEntry.Text = string.Empty;
        adherentCourant = null;
        empruntsActifs = null;

        // Réinitialiser l'affichage
        NomAbonne.Text = string.Empty;
        PrenomAbonne.Text = string.Empty;
        IDAbonne.Text = string.Empty;
        NbLivreEmprunter.Text = string.Empty;
        EmpruntsCollectionView.ItemsSource = null;

        // Afficher le formulaire de recherche
        ListeEmprunts.IsVisible = false;
        RetourContents.IsVisible = true;
        HeaderText.IsVisible = true;

        // Focus sur la saisie
        AbonneIdEntry.Focus();
    }

    /// <summary>
    /// Vers la page d'accueil
    /// </summary>
    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    /// <summary>
    /// Basculer entre la recherche et la liste des emprunts
    /// </summary>
    private void OnFloatingAddAdherentClicked(object sender, EventArgs e)
    {
        ListeEmprunts.IsVisible = !ListeEmprunts.IsVisible;
        RetourContents.IsVisible = !RetourContents.IsVisible;
        HeaderText.IsVisible = !HeaderText.IsVisible;

        if (RetourContents.IsVisible)
        {
            AbonneIdEntry.Focus();
        }
    }

    /// <summary>
    /// Gérer l'apparition de la page
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();
        AbonneIdEntry.Focus();
    }

    /// <summary>
    /// Gérer la disparition de la page
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Nettoyer les ressources si nécessaire
    }    
}