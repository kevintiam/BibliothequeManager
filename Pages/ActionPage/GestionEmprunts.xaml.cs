using BibliothequeManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Pages.ActionPage;

public partial class GestionEmprunts : ContentPage
{
	public GestionEmprunts()
	{
		InitializeComponent();
        FilterPicker.ItemsSource = StatutOptions;

        ChargerEmprunts();

    }

    private void ChargerEmprunts()
    {
        using var donnee = new BibliothequeContext();
        var emprunts = donnee.Emprunts
            .Include(e => e.Adherent)
            .Include(e => e.Exemplaire)
                .ThenInclude(ex => ex.Livre)
            .ToList();

        EmpruntsCollectionView.ItemsSource = emprunts;
    }

    private async void OnAccueilClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new HomePage());
    }

    public List<string> StatutOptions { get; } = new()
    {
        App.Localized["All"],
        App.Localized["InProgress"],
        App.Localized["Late"],
        App.Localized["Returned"]
    };
}