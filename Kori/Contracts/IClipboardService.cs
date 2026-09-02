using System.Threading.Tasks;
using Avalonia.Controls;

namespace Kori.Contracts;

/// <summary>
/// Defines the contract for interacting with the system clipboard.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Initializes the clipboard service with the specified top-level window.
    /// </summary>
    /// <param name="topLevel">The top-level window used to access the clipboard.</param>
    void Initialize(TopLevel topLevel);

    /// <summary>
    /// Places the given text on the clipboard.
    /// </summary>
    /// <param name="text">The text to copy to the clipboard.</param>
    Task SetTextAsync(string text);
}
