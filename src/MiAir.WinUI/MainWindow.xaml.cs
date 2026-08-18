using System;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MiAir.WinUI.Services;
using MiAir.WinUI.ViewModels;
using MiAir.WinUI.Views;
using WinRT.Interop;

namespace MiAir.WinUI;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();
    private AppWindow? _appWindow;

    public MainWindow()
    {
        this.InitializeComponent();

        // Setup Custom Fluent TitleBar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // Initialize AppWindow sizing & position
        InitWindow();

        // Default navigation
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(DashboardPage));

        // Start Core
        _ = ViewModel.InitializeAsync();
    }

    private void InitWindow()
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        if (_appWindow != null)
        {
            _appWindow.Resize(new Windows.Graphics.SizeInt32(960, 680));
            _appWindow.Title = "MiAir for Windows";

            // Intercept window close to minimize to system tray
            _appWindow.Closing += OnWindowClosing;
        }
    }

    private void OnWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (SettingsService.Instance.Settings.MinimizeToTrayOnClose)
        {
            args.Cancel = true;
            _appWindow?.Hide();
        }
        else
        {
            CoreProcessService.Instance.StopCore();
        }
    }

    private void OnNavViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            Type targetPage = tag switch
            {
                "dashboard" => typeof(DashboardPage),
                "devices" => typeof(DevicesPage),
                "settings" => typeof(SettingsPage),
                _ => typeof(DashboardPage)
            };

            if (ContentFrame.CurrentSourcePageType != targetPage)
            {
                ContentFrame.Navigate(targetPage);
            }
        }
    }

    private void OnTrayIconLeftMouseDown(object sender, RoutedEventArgs e)
    {
        ShowAndActivate();
    }

    private void OnShowMainWindowClick(object sender, RoutedEventArgs e)
    {
        ShowAndActivate();
    }

    private void OnExitAppClick(object sender, RoutedEventArgs e)
    {
        CoreProcessService.Instance.StopCore();
        TrayIcon.Dispose();
        Application.Current.Exit();
    }

    public void ShowAndActivate()
    {
        _appWindow?.Show();
        this.Activate();
    }
}
