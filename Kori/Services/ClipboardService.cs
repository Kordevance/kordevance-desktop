using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Kori.Contracts;

namespace Kori.Services;

public sealed class ClipboardService : IClipboardService
{
    private TopLevel? _topLevel;

    public void Initialize(TopLevel topLevel)
    {
        _topLevel = topLevel;
    }

    public async Task SetTextAsync(string text)
    {
        var clipboard = _topLevel?.Clipboard;
        if (clipboard is null)
        {
            return;
        }

        await clipboard.SetTextAsync(text);
    }
}
