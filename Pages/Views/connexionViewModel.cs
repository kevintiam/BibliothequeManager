using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BibliothequeManager.Models;
using BibliothequeManager.Pages;
using BibliothequeManager.Pages.Views;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Views;

// connexion Bilou@biblio.fr mot de passe : 123456

public class ConnexionViewModel : INotifyPropertyChanged
{
    // Contexte de base de données pour accéder aux données
    // saisies par l'utilisateur
    private readonly BibliothequeContext context = new();
   
    private string email = "";
    private string motDePasse = "";
    private string messageErreur = "";  
    private bool estErreurVisible;

    public string Email
    {
        get => email;
        set { email = value; OnPropertyChanged(); }
    }

    public string MotDePasse
    {
        get => motDePasse;
        set { motDePasse = value; OnPropertyChanged(); }
    }

    public string MessageErreur
    {
        get => messageErreur;
        set { messageErreur = value; OnPropertyChanged(); }
    }

    public bool EstErreurVisible
    {
        get => estErreurVisible;
        set { estErreurVisible = value; OnPropertyChanged(); }
    }

    // Commande de connexion
    public ICommand LoginCommand { get; }

    public ConnexionViewModel()
    {
        LoginCommand = new Command(SeConnecter);
    }

    // Méthode pour se connecter
    private async void SeConnecter()
    {
        // Masquer l'erreur au départ
        EstErreurVisible = false;

        // Vérifier si les champs sont vides
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(MotDePasse))
        {
            MessageErreur = "Veuillez remplir tous les champs.";
            EstErreurVisible = true;
            return;
        }

        // Nettoyer les entrées
        string email = Email.Trim();
        string mdp = MotDePasse.Trim();

        try
        {
            // Chercher l'utilisateur dans la base
            var user = await context.Bibliothecaires
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == mdp);

            if (user != null)
            {
                // Connexion réussie, aller à l'accueil
                await Application.Current.MainPage.Navigation.PushModalAsync(new HomePage());
            }
            else
            {
                // Échec, afficher erreur
                MessageErreur = "Email ou mot de passe incorrect.";
                EstErreurVisible = true;
            }
        }
        catch
        {
            // Erreur de base de données
            MessageErreur = "Erreur de connexion à la base.";
            EstErreurVisible = true;
        }
    }

    // Evénement pour la notification de changement de propriété à l'interface
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    }