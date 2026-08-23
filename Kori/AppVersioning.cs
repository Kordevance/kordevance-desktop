using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace Kori;

public sealed class AppVersioning
{
    private const string RepoUrl = "https://github.com/Kordevance/kordevance-desktop";

    public string CurrentVersion { get; private set; } = string.Empty;
    public string? NewVersion { get; private set; }

    private bool _initialized;
    private readonly UpdateManager _mgr = new(new GithubSource(RepoUrl, null, false));
    private UpdateInfo? _pendingUpdate;

    private async Task<bool> DoCheckForUpdates()
    {
        _initialized = true;

        if (!_mgr.IsInstalled)
        {
            return false;
        }

        try
        {
            var newVersion = await _mgr.CheckForUpdatesAsync();
            if (newVersion == null) return false;

            CurrentVersion = _mgr.CurrentVersion?.ToNormalizedString() ?? string.Empty;
            NewVersion = newVersion.TargetFullRelease.Version.ToNormalizedString();

            await _mgr.DownloadUpdatesAsync(newVersion);

            _pendingUpdate = newVersion;
            return true;
        }
        catch
        {
            _initialized = false;
            _pendingUpdate = null;
            return false;
        }
    }

    public async Task<bool> CheckForUpdatesAsync(bool force = false)
    {
        if (force) return await DoCheckForUpdates();
        if (_initialized) return _pendingUpdate != null;

        return await DoCheckForUpdates();
    }

    public void ApplyUpdateAndRestart()
    {
        if (_pendingUpdate == null)
            return;

        try
        {
            _mgr.ApplyUpdatesAndRestart(_pendingUpdate);
        }
        catch
        {
            // ignored
        }
    }

    public bool HasPendingUpdate => _pendingUpdate != null;
}