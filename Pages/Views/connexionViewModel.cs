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
    private readonly BibliothequeContext _context = new();
    private string _email = "";
    private string _motDePasse = "";
    private string _messageErreur = "";
    private bool _estErreurVisible;

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public string MotDePasse
    {
        get => _motDePasse;
        set { _motDePasse = value; OnPropertyChanged(); }
    }

    public string MessageErreur
    {
        get => _messageErreur;
        set { _messageErreur = value; OnPropertyChanged(); }
    }

    public bool EstErreurVisible
    {
        get => _estErreurVisible;
        set { _estErreurVisible = value; OnPropertyChanged(); }
    }

    public ICommand LoginCommand { get; }

    public ConnexionViewModel()
    {
        LoginCommand = new Command(SeConnecter);
    }

    private async void SeConnecter()
    {
        EstErreurVisible = false;

        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(MotDePasse))
        {
            MessageErreur = "Veuillez remplir tous les champs.";
            EstErreurVisible = true;
            return;
        }

        string email = Email.Trim();
        string mdp = MotDePasse.Trim();

        try
        {
            var user = await _context.Bibliothecaires
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == mdp);

            if (user != null)
            {
                // une fois  Connexion réussie / aller à l'accueil
                await Application.Current.MainPage.Navigation.PushModalAsync(new HomePage());
            }
            else
            {
                MessageErreur = "Email ou mot de passe incorrect.";
                EstErreurVisible = true;
            }
        }
        catch
        {
            MessageErreur = "Erreur de connexion à la base.";
            EstErreurVisible = true;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}