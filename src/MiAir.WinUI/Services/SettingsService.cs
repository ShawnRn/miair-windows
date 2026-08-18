using System;
using System.IO;
using System.Text.Json;
using MiAir.WinUI.Models;

namespace MiAir.WinUI.Services;

public class SettingsService
{
    private static readonly Lazy<SettingsService> _instance = new(() => new SettingsService());
    public static SettingsService Instance => _instance.Value;

    private readonly string _settingsFolder;
    private readonly string _settingsPath;
    private AppSettings _settings;

    public AppSettings Settings => _settings;

    public string DataDirectory => _settingsFolder;
    public string TokenStorePath => Path.Combine(_settingsFolder, "token.json");
    public string StatusFilePath => Path.Combine(_settingsFolder, "status.json");

    private SettingsService()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _settingsFolder = Path.Combine(localAppData, "MiAir");
        _settingsPath = Path.Combine(_settingsFolder, "settings.json");

        Directory.CreateDirectory(_settingsFolder);
        _settings = LoadSettings();
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    return loaded;
                }
            }
        }
        catch (Exception)
        {
            // Fall back to default on parse error
        }

        var defaultSettings = new AppSettings();
        SaveSettings(defaultSettings);
        return defaultSettings;
    }

    public void SaveSettings(AppSettings? settings = null)
    {
        if (settings != null)
        {
            _settings = settings;
        }

        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_settings, options);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception)
        {
            // Ignore write errors or log
        }
    }
}
