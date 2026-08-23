using System;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Kori;

public class LocalizeExtension : MarkupExtension
{
    private string Key { get; }

    public LocalizeExtension(string key)
    {
        Key = key;
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new ReflectionBindingExtension($"[{Key}]")
        {
            Mode = BindingMode.OneWay,
            Source = Localizer.Instance,
        };
        return binding.ProvideValue(serviceProvider);
    }
}