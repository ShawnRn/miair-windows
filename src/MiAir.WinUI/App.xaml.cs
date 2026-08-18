using System;
using Microsoft.UI.Xaml;
using MiAir.WinUI.Services;

namespace MiAir.WinUI;

public partial class App : Application
{
    private Window? _mainWindow;

    public static Window? MainWindowInstance { get; private set; }

    public App()
    {
        this.InitializeComponent();
        this.UnhandledException += OnUnhandledException;
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        MainWindowInstance = _mainWindow;
        _mainWindow.Activate();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // Prevent hard crash and keep logging if needed
        e.Handled = true;
    }
}
