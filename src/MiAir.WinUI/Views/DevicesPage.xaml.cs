using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MiAir.WinUI.ViewModels;

namespace MiAir.WinUI.Views;

public sealed partial class DevicesPage : Page
{
    public DevicesViewModel ViewModel { get; } = new();

    public DevicesPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.RefreshDevicesAsync();
    }

    private async void OnOpenQrLoginClick(object sender, RoutedEventArgs e)
    {
        var dialog = new QrLoginDialog
        {
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
        if (dialog.LoginSuccess)
        {
            await ViewModel.RefreshDevicesAsync();
        }
    }

    private void OnBindSpeakerClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is MiAir.WinUI.Models.SpeakerDevice dev)
        {
            _ = ViewModel.SelectDeviceCommand.ExecuteAsync(dev);
        }
    }
}
