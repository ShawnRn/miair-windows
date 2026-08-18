using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiAir.WinUI.Models;
using MiAir.WinUI.Services;

namespace MiAir.WinUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _deviceName = "小爱音箱投放";

    [ObservableProperty]
    private bool _airPlayEnabled = true;

    [ObservableProperty]
    private int _airPlayPort = 5000;

    [ObservableProperty]
    private int _httpPort = 8300;

    [ObservableProperty]
    private int _bufferMs = 500;

    [ObservableProperty]
    private bool _dlnaEnabled = true;

    [ObservableProperty]
    private int _dlnaPort = 8301;

    [ObservableProperty]
    private string _sourcePolicy = "latest";

    [ObservableProperty]
    private int _idleTimeout = 10;

    [ObservableProperty]
    private string _preferredProtocol = "airplay";

    [ObservableProperty]
    private bool _startOnBoot;

    [ObservableProperty]
    private bool _minimizeToTrayOnClose = true;

    [ObservableProperty]
    private string _appTheme = "Default";

    [ObservableProperty]
    private string _saveStatusText = string.Empty;

    public SettingsViewModel()
    {
        LoadSettings();
    }

    public void LoadSettings()
    {
        var s = SettingsService.Instance.Settings;
        DeviceName = s.DeviceName;
        AirPlayEnabled = s.AirPlayEnabled;
        AirPlayPort = s.AirPlayPort;
        HttpPort = s.HttpPort;
        BufferMs = s.BufferMs;
        DlnaEnabled = s.DlnaEnabled;
        DlnaPort = s.DlnaPort;
        SourcePolicy = s.SourcePolicy;
        IdleTimeout = s.IdleTimeout;
        PreferredProtocol = s.PreferredProtocol;
        StartOnBoot = StartupService.IsStartOnBootEnabled();
        MinimizeToTrayOnClose = s.MinimizeToTrayOnClose;
        AppTheme = s.AppTheme;
    }

    [RelayCommand]
    private async Task SaveAndApplyAsync()
    {
        var s = SettingsService.Instance.Settings;
        s.DeviceName = DeviceName;
        s.AirPlayEnabled = AirPlayEnabled;
        s.AirPlayPort = AirPlayPort;
        s.HttpPort = HttpPort;
        s.BufferMs = BufferMs;
        s.DlnaEnabled = DlnaEnabled;
        s.DlnaPort = DlnaPort;
        s.SourcePolicy = SourcePolicy;
        s.IdleTimeout = IdleTimeout;
        s.PreferredProtocol = PreferredProtocol;
        s.MinimizeToTrayOnClose = MinimizeToTrayOnClose;
        s.AppTheme = AppTheme;

        StartupService.SetStartOnBoot(StartOnBoot);
        s.StartOnBoot = StartOnBoot;

        SettingsService.Instance.SaveSettings(s);

        SaveStatusText = "正在应用配置并重载核心服务...";
        await CoreProcessService.Instance.RestartCoreAsync(s);
        SaveStatusText = "✓ 配置已保存并立即生效";
    }
}
