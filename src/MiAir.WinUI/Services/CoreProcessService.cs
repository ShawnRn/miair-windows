using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MiAir.WinUI.Models;

namespace MiAir.WinUI.Services;

public class CoreProcessService
{
    private static readonly Lazy<CoreProcessService> _instance = new(() => new CoreProcessService());
    public static CoreProcessService Instance => _instance.Value;

    private Process? _process;
    private readonly object _lock = new();
    private bool _isStopping;

    public bool IsRunning => _process != null && !_process.HasExited;

    private CoreProcessService()
    {
    }

    public string FindCoreExecutable()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var directExe = Path.Combine(baseDir, "miair-core.exe");
        if (File.Exists(directExe))
        {
            return directExe;
        }

        var isArm64 = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
        var archSubdir = isArm64 ? "win-arm64" : "win-x64";

        var runtimeExe = Path.Combine(baseDir, "runtimes", archSubdir, "miair-core.exe");
        if (File.Exists(runtimeExe))
        {
            return runtimeExe;
        }

        // Also check development build output directory
        var devExe = Path.Combine(baseDir, "..", "..", "..", "..", "..", "core", "bin", archSubdir, "miair-core.exe");
        if (File.Exists(devExe))
        {
            return Path.GetFullPath(devExe);
        }

        return directExe;
    }

    public async Task<bool> StartCoreAsync(AppSettings settings)
    {
        lock (_lock)
        {
            if (IsRunning)
            {
                return true;
            }

            var exePath = FindCoreExecutable();
            if (!File.Exists(exePath))
            {
                return false;
            }

            var settingsService = SettingsService.Instance;
            var args = $" -name \"{settings.DeviceName}\"" +
                       $" -airplay={settings.AirPlayEnabled.ToString().ToLower()}" +
                       $" -port={settings.AirPlayPort}" +
                       $" -http-port={settings.HttpPort}" +
                       $" -buffer-ms={settings.BufferMs}" +
                       $" -dlna={settings.DlnaEnabled.ToString().ToLower()}" +
                       $" -dlna-port={settings.DlnaPort}" +
                       $" -source-policy=\"{settings.SourcePolicy}\"" +
                       $" -idle-timeout={settings.IdleTimeout}" +
                       $" -preferred-protocol=\"{settings.PreferredProtocol}\"" +
                       $" -store=\"{settingsService.TokenStorePath}\"" +
                       $" -status-file=\"{settingsService.StatusFilePath}\"" +
                       $" -api=true" +
                       $" -api-port=8302";

            if (!string.IsNullOrWhiteSpace(settings.SelectedDid))
            {
                args += $" -device=\"{settings.SelectedDid}\"";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = settingsService.DataDirectory
            };

            try
            {
                _process = Process.Start(startInfo);
                if (_process != null)
                {
                    _process.EnableRaisingEvents = true;
                    _process.Exited += OnProcessExited;
                }
            }
            catch
            {
                return false;
            }
        }

        // Wait up to 3 seconds for REST API readiness
        for (int i = 0; i < 15; i++)
        {
            if (await ApiClient.Instance.IsCoreReachableAsync())
            {
                return true;
            }
            await Task.Delay(200);
        }

        return IsRunning;
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        if (_isStopping) return;

        // Auto restart if died unexpectedly
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            if (!_isStopping)
            {
                await StartCoreAsync(SettingsService.Instance.Settings);
            }
        });
    }

    public async Task RestartCoreAsync(AppSettings settings)
    {
        StopCore();
        await Task.Delay(300);
        await StartCoreAsync(settings);
    }

    public void StopCore()
    {
        lock (_lock)
        {
            _isStopping = true;
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill(true);
                    _process.WaitForExit(1000);
                }
            }
            catch
            {
                // Ignore kill errors
            }
            finally
            {
                _process = null;
                _isStopping = false;
            }
        }
    }
}
