using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Kori.Converters;

public sealed class IsNotZeroConverter : IValueConverter
{
    public static readonly IsNotZeroConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int count && count != 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
