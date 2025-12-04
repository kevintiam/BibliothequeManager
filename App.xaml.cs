using BibliothequeManager.Pages;
using BibliothequeManager.Views;
using Microsoft.Maui.Controls;

namespace BibliothequeManager
{
    public partial class App : Application
    {

        public static LocalizedStrings Localized { get; } = new();
        public App()
        {
            InitializeComponent();

            // MainPage = new NavigationPage(new HomePage());
            MainPage = new NavigationPage(new Connexion());
        }


        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}