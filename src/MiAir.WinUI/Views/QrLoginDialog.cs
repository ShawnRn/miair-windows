using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MiAir.WinUI.Services;

namespace MiAir.WinUI.Views;

public class QrLoginDialog : ContentDialog
{
    private readonly Image _qrImage;
    private readonly ProgressRing _progressRing;
    private readonly TextBlock _statusHint;
    private CancellationTokenSource? _pollCts;

    public bool LoginSuccess { get; private set; }

    public QrLoginDialog()
    {
        Title = "扫码登录小米账号";
        CloseButtonText = "取消";
        DefaultButton = ContentDialogButton.None;
        CornerRadius = new CornerRadius(12);

        var rootPanel = new StackPanel
        {
            Spacing = 16,
            Width = 300,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 12, 0, 8)
        };

        var border = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
            Padding = new Thickness(12),
            HorizontalAlignment = HorizontalAlignment.Center,
            Width = 240,
            Height = 240
        };

        var grid = new Grid();

        _qrImage = new Image
        {
            Stretch = Microsoft.UI.Xaml.Media.Stretch.Uniform,
            Visibility = Visibility.Collapsed
        };
        grid.Children.Add(_qrImage);

        _progressRing = new ProgressRing
        {
            IsActive = true,
            Width = 40,
            Height = 40
        };
        grid.Children.Add(_progressRing);

        border.Child = grid;
        rootPanel.Children.Add(border);

        _statusHint = new TextBlock
        {
            Text = "正在生成登录二维码...",
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
        rootPanel.Children.Add(_statusHint);

        var subHint = new TextBlock
        {
            Text = "请使用【米家 App】或【微信】扫描上方二维码授权登录",
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.7
        };
        rootPanel.Children.Add(subHint);

        Content = rootPanel;

        Opened += OnOpened;
        Closed += OnClosed;
    }

    private async void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        _pollCts = new CancellationTokenSource();
        var ct = _pollCts.Token;

        try
        {
            var qrInfo = await ApiClient.Instance.GetQrCodeAsync(ct);
            if (qrInfo != null && !string.IsNullOrEmpty(qrInfo.Qr))
            {
                _qrImage.Source = new BitmapImage(new Uri(qrInfo.Qr));
                _qrImage.Visibility = Visibility.Visible;
                _progressRing.IsActive = false;
                _progressRing.Visibility = Visibility.Collapsed;
                _statusHint.Text = "请使用手机扫码并确认登录";

                _ = Task.Run(async () =>
                {
                    while (!ct.IsCancellationRequested)
                    {
                        await Task.Delay(2000, ct);
                        if (ct.IsCancellationRequested) break;

                        var poll = await ApiClient.Instance.PollQrCodeAsync(qrInfo.Lp, ct);
                        if (poll != null && poll.Status == "success")
                        {
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                LoginSuccess = true;
                                _statusHint.Text = "✓ 登录成功！正在同步音箱设备...";
                                Hide();
                            });
                            break;
                        }
                    }
                }, ct);
            }
            else
            {
                _statusHint.Text = $"获取二维码失败: {qrInfo?.Error ?? "未知错误"}";
                _progressRing.IsActive = false;
            }
        }
        catch (Exception ex)
        {
            _statusHint.Text = $"异常: {ex.Message}";
            _progressRing.IsActive = false;
        }
    }

    private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _pollCts?.Cancel();
        _pollCts = null;
    }
}
