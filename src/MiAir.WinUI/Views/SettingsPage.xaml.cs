using Microsoft.UI.Xaml.Controls;
using MiAir.WinUI.ViewModels;

namespace MiAir.WinUI.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }
}
