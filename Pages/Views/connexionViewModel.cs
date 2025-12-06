using BibliothequeManager.Models;
using BibliothequeManager.Pages;
using BibliothequeManager.Pages.Views;
using BibliothequeManager.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BibliothequeManager.Views;

// connexion Bilou@biblio.fr mot de passe : 123456

public class ConnexionViewModel : INotifyPropertyChanged
{
    // Contexte de base de données pour accéder aux données
    // saisies par l'utilisateur
    private readonly BibliothequeContext context = new();
    private readonly SessionUser session;

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

    public ConnexionViewModel(SessionUser sessionUser)
    {
        session = sessionUser;
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
        string motDePasseSaisi = MotDePasse.Trim();

        try
        {
            var user = await context.Bibliothecaires
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user != null && !string.IsNullOrEmpty(motDePasseSaisi))
            {
                bool motDePasseValide = BCrypt.Net.BCrypt.Verify(motDePasseSaisi, user.PasswordHash);

                if(motDePasseValide) 
                {
                    var utilisateurConnecte = new Bibliothecaire
                    {
                        Id = user.Id,
                        Nom = user.Nom,
                        Prenom = user.Prenom,
                        Email = user.Email
                    };
                    // Stocker l'utilisateur connecté dans la session
                    session.SeConnecter(utilisateurConnecte);

                    await Application.Current.MainPage.Navigation.PushModalAsync(new HomePage(session));
                }
                else
                {
                    // Échec, afficher erreur
                    MessageErreur = "Email ou mot de passe incorrect.";
                    EstErreurVisible = true;
                }
            }
        }
        catch
        {
            MessageErreur = "Erreur de connexion à la base.";
            EstErreurVisible = true;
        }

        MotDePasse = "";
    }

    // Evénement pour la notification de changement de propriété à l'interface
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    }