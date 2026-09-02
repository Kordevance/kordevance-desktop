using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Kori.Converters;

public sealed class EnumMatchConverter : IValueConverter
{
    public static readonly EnumMatchConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
        {
            return false;
        }

        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.Ordinal);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
