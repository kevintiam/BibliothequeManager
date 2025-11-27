using System.ComponentModel;
using System.Globalization;
using System.Resources;
using BibliothequeManager.Properties;

namespace BibliothequeManager;

public class LocalizedStrings : INotifyPropertyChanged
{
    private static readonly ResourceManager ResourceManager = AppResources.ResourceManager;

    public string this[string key]
    {
        get
        {
            var value = ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
            return string.IsNullOrEmpty(value) ? $"[{key}]" : value;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnCultureChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }
}