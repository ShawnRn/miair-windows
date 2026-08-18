using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiAir.WinUI.Models;
using MiAir.WinUI.Services;
using Microsoft.UI.Dispatching;

namespace MiAir.WinUI.ViewModels;

public partial class DevicesViewModel : ObservableObject
{
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private string _userId = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private SpeakerDevice? _selectedDevice;

    public ObservableCollection<SpeakerDevice> Devices { get; } = new();

    public DevicesViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
    }

    public async Task RefreshDevicesAsync()
    {
        IsLoading = true;
        StatusMessage = "正在获取设备列表...";

        try
        {
            var res = await ApiClient.Instance.GetDevicesAsync();
            _dispatcherQueue.TryEnqueue(() =>
            {
                Devices.Clear();
                if (res?.Devices != null && res.Devices.Count > 0)
                {
                    IsLoggedIn = true;
                    var currentDid = SettingsService.Instance.Settings.SelectedDid;

                    foreach (var dev in res.Devices)
                    {
                        dev.IsSelected = (dev.DeviceID == currentDid);
                        Devices.Add(dev);
                    }

                    SelectedDevice = Devices.FirstOrDefault(d => d.IsSelected) ?? Devices.FirstOrDefault();
                    StatusMessage = $"成功发现 {Devices.Count} 台音箱设备";
                }
                else
                {
                    if (!string.IsNullOrEmpty(res?.Error))
                    {
                        StatusMessage = $"获取音箱失败: {res.Error} (请尝试扫码登录)";
                    }
                    else
                    {
                        StatusMessage = "名下暂无可用小爱音箱设备";
                    }
                }
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"请求异常: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SelectDeviceAsync(SpeakerDevice device)
    {
        if (device == null) return;

        foreach (var d in Devices)
        {
            d.IsSelected = (d.DeviceID == device.DeviceID);
        }

        SelectedDevice = device;
        var settings = SettingsService.Instance.Settings;
        settings.SelectedDid = device.DeviceID;
        settings.SelectedSpeakerName = device.DisplayName;
        SettingsService.Instance.SaveSettings();

        await ApiClient.Instance.BindSpeakerAsync(device.DeviceID);
        StatusMessage = $"已成功绑定: {device.DisplayName}";
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await ApiClient.Instance.LogoutAccountAsync();
        IsLoggedIn = false;
        UserId = string.Empty;
        Devices.Clear();
        StatusMessage = "已注销小米账号";
    }
}
