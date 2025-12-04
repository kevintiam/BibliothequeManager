using BibliothequeManager.Models;
using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Globalization;

namespace BibliothequeManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                //Charger le Community Toolkit MAUI
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                    fonts.AddFont("FontAwesome6Solid.otf", "FontAwesomeSolid");


                    // PlaypenSans – toutes les variantes
                    fonts.AddFont("PlaypenSans-Regular.ttf", "PlaypenSansRegular");
                    fonts.AddFont("PlaypenSans-Bold.ttf", "PlaypenSansBold");
                    fonts.AddFont("PlaypenSans-ExtraBold.ttf", "PlaypenSansExtraBold");
                    fonts.AddFont("PlaypenSans-ExtraLight.ttf", "PlaypenSansExtraLight");
                    fonts.AddFont("PlaypenSans-Light.ttf", "PlaypenSansLight");
                    fonts.AddFont("PlaypenSans-Medium.ttf", "PlaypenSansMedium");
                    fonts.AddFont("PlaypenSans-SemiBold.ttf", "PlaypenSansSemiBold");
                    fonts.AddFont("PlaypenSans-Thin.ttf", "PlaypenSansThin");
                    fonts.AddFont("PlaypenSans-VariableFont_wght.ttf", "PlaypenSansVariable");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // === 🔑 ENREGISTRER LE CONTEXT EF CORE ===
            string connectionString = "Server=KEVINTIAM;Database=BibliothequeLiVraNova;Integrated Security=True;TrustServerCertificate=True;";
            builder.Services.AddDbContext<BibliothequeContext>(options =>
                options.UseSqlServer(connectionString));

            // Langue par défaut : français
            var culture = new CultureInfo("fr");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            return builder.Build();
        }
    }

    public static class SeedData
    {
        public static async Task InitializeAdminAsync(BibliothequeContext context)
        {
            // Ne pas recréer si déjà présent
            if (await context.Bibliothecaires.AnyAsync(b => b.Email == "admin@biblio.local"))
                return;

            var admin = new Bibliothecaire
            {
                Nom = "Admin",
                Prenom = "Bibliothèque",
                Email = "admin@biblio.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Kevin@25!")
            };

            context.Bibliothecaires.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
