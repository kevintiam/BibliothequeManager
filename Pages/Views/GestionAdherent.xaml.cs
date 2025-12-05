using BibliothequeManager.Models;
using BibliothequeManager.Pages.Popups;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BibliothequeManager.Pages.Views;

public partial class GestionAdherent : ContentPage
{
	public GestionAdherent()
	{
		InitializeComponent();        
        AdherentsCollectionView.SelectionChanged += OnAdherentSelectionChanged;
        ChargerAdherents();


    }

    private void OnFloatingAddAdherentClicked(object sender, EventArgs e)
    {
        ViderFormulaire();
        FormulaireAdherent.IsVisible = !FormulaireAdherent.IsVisible;
    }
    
    public void ChargerAdherents()
    {
        using var donnees = new BibliothequeContext();
        try
        {
            var adherents = donnees.Adherents
                .OrderBy(a => a.Nom)
                .ThenBy(a => a.Prenom)
                .AsNoTracking()
                .ToList();
            AdherentsCollectionView.ItemsSource = adherents;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
        }
    }
    private async void btnAjouter_Clicked(object sender, EventArgs e)
    {
        var nom = TxtNomAdherent.Text.Trim();
        var prenom = TxtPrenomAdherent.Text.Trim();
        var email = TxtEmailAdherent.Text.Trim();

        if(string.IsNullOrEmpty(nom) || string.IsNullOrEmpty(prenom) || string.IsNullOrEmpty(email))
        {
            await ErrorPopup.Show("Entrez tous les champs", this);
            return;
        }

        using var donnees = new BibliothequeContext();

        if(await donnees.Adherents.AnyAsync(ad => ad.Email == email))
        {
            await ErrorPopup.Show("Cet email est deja utilise", this);
            return;
        }
        string numeroCarte;
        Adherent? adherentExiste;
        do
        {
            numeroCarte = Adherent.GenererNumeroUnique();
            adherentExiste = await donnees.Adherents.FirstOrDefaultAsync(a => a.NumeroCarte == numeroCarte);
        }
        while (adherentExiste != null);
        var newAdherent = new Adherent
        {
           Nom = nom,
           Prenom = prenom,
           Email = email,
           NumeroCarte = numeroCarte,
           Amandes = 0
        };
        donnees.Adherents.Add(newAdherent);
        await donnees.SaveChangesAsync();

        ChargerAdherents();
        await SuccessPopup.Show("Adherent ajoute avec succes", this);

    }

    private void btnAnnuler_Clicked(object sender, EventArgs e)
    {
        ViderFormulaire();
        FormulaireAdherent.IsVisible = !FormulaireAdherent.IsVisible;
        

    }

    private void ViderFormulaire()
    {
        TxtNomAdherent.Text = string.Empty;
        TxtPrenomAdherent.Text = string.Empty;
        TxtEmailAdherent.Text = string.Empty;
        TxtNumeroCarte.Text = string.Empty;
    }

    private async void OnAdherentSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var Adherent = e.CurrentSelection.FirstOrDefault() as Adherent;
        if (Adherent != null)
        {
            using var donnee = new BibliothequeContext();
            var AdherentSelectionnee = await donnee.Adherents
                .Include(c => c.Emprunts)
                .FirstOrDefaultAsync(c => c.Id == Adherent.Id);

            if (AdherentSelectionnee != null)
            {
                TxtNomAdherent.Text = AdherentSelectionnee.Nom;
                TxtPrenomAdherent.Text = AdherentSelectionnee.Prenom;
                TxtEmailAdherent.Text = AdherentSelectionnee.Email;
                TxtAmande.Text = $"{AdherentSelectionnee.Amandes.ToString()} $CAD" ;
                TxtNumeroCarte.Text = AdherentSelectionnee.NumeroCarte;
                ModifierButton.IsVisible = true;
                SupprimerButton.IsVisible = true;
                btnAjouter.IsVisible = false;
                btnAnnuler.IsVisible = false;

                Carte.IsVisible = true;
                Amande.IsVisible = true;
                FormulaireAdherent.IsVisible = true;
            }
        }
    }
    private async void ModifierButton_Clicked(object sender, EventArgs e)
    {
        var selectedAdherent = AdherentsCollectionView.SelectedItem as Adherent;
        if (selectedAdherent == null) return;

        int idAdherent = selectedAdherent.Id;

        var nom = TxtNomAdherent.Text?.Trim();
        var prenom = TxtPrenomAdherent.Text?.Trim();
        var email = TxtEmailAdherent.Text?.Trim();
        if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrEmpty(prenom) || string.IsNullOrEmpty(email))
        {
            await ErrorPopup.Show(App.Localized["popErrorCategori1"], this);
            return;
        }

        using var context = new BibliothequeContext();


        var adherent = await context.Adherents.FindAsync(idAdherent);

        if (adherent != null)
        {
            adherent.Nom = nom;
            adherent.Prenom = prenom;
            adherent.Email = email;
            await context.SaveChangesAsync();
            await SuccessPopup.Show(App.Localized["succespopCategorie1"], this);
            ChargerAdherents();
            ViderFormulaire();
            FormulaireAdherent.IsVisible = false;
            AdherentsCollectionView.SelectedItem = null;
        }

    }

    private async void SupprimerButton_Clicked(object sender, EventArgs e)
    {
        var selectedAdherent = AdherentsCollectionView.SelectedItem as Adherent;

        if (selectedAdherent != null)
        {
            var popup = new ConfirmationPopup
            {
                Title = App.Localized["ConfirmDelete"],
                Message = string.Format(App.Localized["popDelAuthor"], selectedAdherent.Prenom, selectedAdherent.Nom)
            };

            popup.OnCompleted += async (confirmed) =>
            {
                if (confirmed)
                {
                    int idAdherent = selectedAdherent.Id;
                    using var context = new BibliothequeContext();
                    var adherent = await context.Adherents.FindAsync(idAdherent);
                    if (adherent != null)
                    {
                        context.Adherents.Remove(adherent);
                        await context.SaveChangesAsync();
                        await SuccessPopup.Show(App.Localized["AuthorSuccessfullyDeleted!"], this);
                        ChargerAdherents();
                        ViderFormulaire();
                        FormulaireAdherent.IsVisible = false;
                        AdherentsCollectionView.SelectedItem = null;
                    }
                }
            };

            await Navigation.PushModalAsync(popup);
        }

    }
}