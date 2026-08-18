using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiAir.WinUI.Models;
using MiAir.WinUI.Services;

namespace MiAir.WinUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "MiAir for Windows";

    [ObservableProperty]
    private string _statusSummary = "正在初始化...";

    [ObservableProperty]
    private bool _isStreaming;

    [ObservableProperty]
    private bool _isCoreRunning;

    public MainViewModel()
    {
    }

    public async Task InitializeAsync()
    {
        var settings = SettingsService.Instance.Settings;
        IsCoreRunning = await CoreProcessService.Instance.StartCoreAsync(settings);
    }
}
