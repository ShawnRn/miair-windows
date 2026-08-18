using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MiAir.WinUI.ViewModels;

namespace MiAir.WinUI.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; } = new();

    public DashboardPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.StartPolling();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.StopPolling();
    }

    private void OnVolumeSliderValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        _ = ViewModel.ChangeVolumeCommand.ExecuteAsync(e.NewValue);
    }
}
