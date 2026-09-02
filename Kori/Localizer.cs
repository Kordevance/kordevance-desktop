using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform;

namespace Kori;

public class Localizer: INotifyPropertyChanged
{
    private Dictionary<string, string> _strings = new();
    
    public static Localizer? Instance { get; private set; }
    
    #pragma warning disable CS0067
    public event PropertyChangedEventHandler? PropertyChanged;
    #pragma warning restore CS0067
    
    // Indexer
    public string this[string key] =>
        _strings.TryGetValue(key, out var value) ? value : $"[{key}]";
    
    public static Task Initialize()
    {
        Instance = new Localizer();
        return Task.CompletedTask;
    }
    
    private Localizer()
    {
        if (!TryLoad(CultureInfo.CurrentUICulture.Name) &&
            !TryLoad(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName))
            TryLoad("en");
    }

    private bool TryLoad(string language)
    {
        var uri = new Uri($"avares://Kori/Assets/i18n/{language}.json");
        try
        {
            using var stream = AssetLoader.Open(uri);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd()) ?? new();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
}