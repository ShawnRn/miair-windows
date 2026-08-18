using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiAir.WinUI.Models;
using MiAir.WinUI.Services;
using Microsoft.UI.Dispatching;

namespace MiAir.WinUI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DispatcherQueue _dispatcherQueue;
    private CancellationTokenSource? _pollCts;

    [ObservableProperty]
    private bool _isCoreRunning;

    [ObservableProperty]
    private string _version = "1.1.2";

    [ObservableProperty]
    private string _deviceName = "小爱音箱投放";

    [ObservableProperty]
    private string _targetSpeaker = "未绑定 (自动识别名下首台音箱)";

    [ObservableProperty]
    private bool _isStreaming;

    [ObservableProperty]
    private string _activeProtocol = string.Empty;

    [ObservableProperty]
    private string _clientDevice = string.Empty;

    [ObservableProperty]
    private string _streamDuration = "00:00:00";

    [ObservableProperty]
    private string _statusBadgeText = "空闲就绪";

    [ObservableProperty]
    private bool _hasToken;

    [ObservableProperty]
    private bool _isTokenValid;

    [ObservableProperty]
    private string _tokenStatusText = "未登录小米账号";

    [ObservableProperty]
    private double _volume = 50;

    public DashboardViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public void StartPolling()
    {
        _pollCts?.Cancel();
        _pollCts = new CancellationTokenSource();
        var ct = _pollCts.Token;

        _ = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var status = await ApiClient.Instance.GetStatusAsync(ct);
                    _dispatcherQueue.TryEnqueue(() => UpdateStatus(status));
                }
                catch
                {
                    // Ignore transient network errors
                }

                await Task.Delay(1000, ct);
            }
        }, ct);
    }

    public void StopPolling()
    {
        _pollCts?.Cancel();
        _pollCts = null;
    }

    private void UpdateStatus(StreamStatusResponse? status)
    {
        if (status == null)
        {
            IsCoreRunning = CoreProcessService.Instance.IsRunning;
            StatusBadgeText = "核心服务启动中...";
            return;
        }

        IsCoreRunning = status.Running;
        Version = status.Version;
        DeviceName = status.Config.Name;

        var settings = SettingsService.Instance.Settings;
        TargetSpeaker = string.IsNullOrWhiteSpace(settings.SelectedSpeakerName)
            ? (string.IsNullOrWhiteSpace(status.Config.TargetDid) ? "自动选择第一个音箱" : status.Config.TargetDid)
            : settings.SelectedSpeakerName;

        if (status.Token != null)
        {
            HasToken = status.Token.HasCredentials;
            IsTokenValid = status.Token.Valid;
            if (HasToken && IsTokenValid)
            {
                var lastRefresh = status.Token.LastRefresh.HasValue
                    ? status.Token.LastRefresh.Value.ToLocalTime().ToString("HH:mm:ss")
                    : "已就绪";
                TokenStatusText = $"✓ 已授权 (保活时间: {lastRefresh})";
            }
            else if (HasToken && !IsTokenValid)
            {
                TokenStatusText = $"! 授权凭据异常 ({status.Token.LastError ?? "请重新登录"})";
            }
            else
            {
                TokenStatusText = "未登录小米账号";
            }
        }
        else
        {
            HasToken = false;
            IsTokenValid = false;
            TokenStatusText = "未登录小米账号";
        }

        var active = status.Source.Active;
        if (active != null && !string.IsNullOrEmpty(active.Id))
        {
            IsStreaming = true;
            ActiveProtocol = active.Protocol.ToUpperInvariant();
            ClientDevice = active.Device;
            var elapsed = DateTime.UtcNow - active.StartedAt;
            if (elapsed < TimeSpan.Zero) elapsed = TimeSpan.Zero;
            StreamDuration = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            StatusBadgeText = $"● {ActiveProtocol} 正在投播中";
        }
        else
        {
            IsStreaming = false;
            ActiveProtocol = string.Empty;
            ClientDevice = string.Empty;
            StreamDuration = "00:00:00";
            StatusBadgeText = "空闲 · 等待设备连接";
        }
    }

    [RelayCommand]
    private async Task PausePlaybackAsync()
    {
        var settings = SettingsService.Instance.Settings;
        await ApiClient.Instance.PauseSpeakerAsync(settings.SelectedDid);
    }

    [RelayCommand]
    private async Task ChangeVolumeAsync(double newVolume)
    {
        var volInt = (int)Math.Clamp(newVolume, 0, 100);
        Volume = volInt;
        var settings = SettingsService.Instance.Settings;
        await ApiClient.Instance.SetVolumeAsync(settings.SelectedDid, volInt);
    }
}
