using BibliothequeManager.Models;
using BibliothequeManager.Pages;
using BibliothequeManager.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls;

namespace BibliothequeManager
{
    public partial class App : Application
    {

        public static LocalizedStrings Localized { get; } = new();
        public App()
        {
            InitializeComponent();

            //MainPage = new NavigationPage(new HomePage());
            MainPage = new NavigationPage(new Connexion());
        }


        protected override async void OnStart()
        {
            base.OnStart();
            try
            {
                using var scope = MauiProgram.CreateMauiApp().Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<BibliothequeContext>();
                await context.Database.MigrateAsync();
                await SeedData.InitializeAdminAsync(context);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur SeedData : {ex}");
            }
        }
    }
}