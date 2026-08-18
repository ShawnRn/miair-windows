using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace MiAir.WinUI.Services;

public static class StartupService
{
    private const string AppName = "MiAir";
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool IsStartOnBootEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            var value = key?.GetValue(AppName);
            return value != null;
        }
        catch
        {
            return false;
        }
    }

    public static bool SetStartOnBoot(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null) return false;

            if (enable)
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\" --autostart");
                    return true;
                }
                return false;
            }
            else
            {
                key.DeleteValue(AppName, false);
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}
