using System;
using System.IO;
using H.NotifyIcon;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MiAir.WinUI.Services;
using MiAir.WinUI.ViewModels;
using MiAir.WinUI.Views;
using WinRT.Interop;

namespace MiAir.WinUI;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();
    private AppWindow? _appWindow;
    private TaskbarIcon? _trayIcon;

    public MainWindow()
    {
        this.InitializeComponent();

        // Setup Custom Fluent TitleBar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // Initialize AppWindow sizing & position
        InitWindow();

        // Setup System Tray Icon
        InitTrayIcon();

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

    private void InitTrayIcon()
    {
        try
        {
            _trayIcon = new TaskbarIcon
            {
                ToolTipText = "MiAir 小爱音箱投播"
            };

            var menu = new MenuFlyout();

            var showItem = new MenuFlyoutItem { Text = "显示主窗口" };
            showItem.Click += (s, e) => ShowAndActivate();
            menu.Items.Add(showItem);

            menu.Items.Add(new MenuFlyoutSeparator());

            var exitItem = new MenuFlyoutItem { Text = "退出 MiAir" };
            exitItem.Click += (s, e) => ExitApp();
            menu.Items.Add(exitItem);

            _trayIcon.ContextFlyout = menu;
            _trayIcon.TrayLeftMouseDown += (s, e) => ShowAndActivate();
            _trayIcon.ForceCreate();
        }
        catch
        {
            // Fallback if tray icon cannot be created
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
            ExitApp();
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

    public void ShowAndActivate()
    {
        _appWindow?.Show();
        this.Activate();
    }

    public void ExitApp()
    {
        CoreProcessService.Instance.StopCore();
        try
        {
            _trayIcon?.Dispose();
        }
        catch { }
        Application.Current.Exit();
    }
}
