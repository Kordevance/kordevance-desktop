using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Kori.Models;

namespace Kori.Converters;

public sealed class ConversationTitleConverter : IValueConverter
{
    public static readonly ConversationTitleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Conversation conversation)
        {
            return string.Empty;
        }

        var first = conversation.Messages.FirstOrDefault(m => m.Role == ChatRole.User)?.Content
                    ?? conversation.Messages.FirstOrDefault()?.Content;

        if (string.IsNullOrWhiteSpace(first))
        {
            return "New conversation";
        }

        return first.Length > 40 ? first[..40] + "…" : first;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
