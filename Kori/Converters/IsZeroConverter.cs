using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Kori.Converters;

public sealed class IsZeroConverter : IValueConverter
{
    public static readonly IsZeroConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int count && count == 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
