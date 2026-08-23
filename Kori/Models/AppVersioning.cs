// using System.Threading.Tasks;
// using Velopack;
//
// namespace Kori.Models;
//
// public sealed class AppVersioning()
// {
//     public string CurrentVersion { get; private set; } = string.Empty;
//     public string? NewVersion { get; private set; }
//     
//     private bool _initialized;
//     private UpdateManager? _mgr;
//     private UpdateInfo? _pendingUpdate;
//
//     private async Task<bool> DoCheckForUpdates(string accessToken)
//     {
//         _initialized = true;
//         
//         //var server = await authHandler.HandleFetchUpdateServer(accessToken);
//         if (string.IsNullOrEmpty(server)) return false;
//
//         try
//         {
//             _mgr = new UpdateManager(server);
//
//             var newVersion = await _mgr.CheckForUpdatesAsync();
//             if (newVersion == null) return false;
//             
//             CurrentVersion = _mgr.CurrentVersion?.ToNormalizedString() ?? string.Empty;
//             NewVersion = newVersion.TargetFullRelease.Version.ToNormalizedString();
//
//             await _mgr.DownloadUpdatesAsync(newVersion);
//
//             _pendingUpdate = newVersion;
//             return true;
//         }
//         catch
//         {
//             _initialized = false;
//             _pendingUpdate = null;
//             return false;
//         }
//     }
//
//     public async Task<bool> CheckForUpdatesAsync(string accessToken, bool force = false)
//     {
//         if (force) return await DoCheckForUpdates(accessToken);
//         if (_initialized) return _pendingUpdate != null;
//
//         return await DoCheckForUpdates(accessToken);
//     }
//     
//     public void ApplyUpdateAndRestart()
//     {
//         if (_mgr == null || _pendingUpdate == null)
//             return;
//
//         try
//         {
//             _mgr.ApplyUpdatesAndRestart(_pendingUpdate);
//         }
//         catch
//         {
//             // ignored
//         }
//     }
//
//     public bool HasPendingUpdate => _pendingUpdate != null;
//     
// }