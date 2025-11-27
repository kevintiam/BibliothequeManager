using System.Globalization;
using Microsoft.Maui.Controls;

namespace BibliothequeManager
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnSwitchLanguageClicked(object sender, EventArgs e)
        {
            // Détecte la langue actuelle
            string currentLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            string newLang = currentLang == "fr" ? "en" : "fr";

            // Change la culture
            var culture = new CultureInfo(newLang);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            // Recharge la page pour appliquer les traductions
            // On utilise une petite astuce : on navigue vers la même page
            string route = Shell.Current?.CurrentState?.Location?.ToString() ?? "//MainPage";
            _ = Shell.Current?.GoToAsync($"//{route}");
        }
    }
}