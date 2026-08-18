using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MiAir.WinUI.Services;

namespace MiAir.WinUI.Views;

public sealed partial class QrLoginDialog : ContentDialog
{
    private CancellationTokenSource? _pollCts;
    public bool LoginSuccess { get; private set; }

    public QrLoginDialog()
    {
        this.InitializeComponent();
        this.Opened += OnOpened;
        this.Closed += OnClosed;
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
                QrImage.Source = new BitmapImage(new Uri(qrInfo.Qr));
                QrImage.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                QrProgressRing.IsActive = false;
                QrProgressRing.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                StatusHintText.Text = "请使用手机扫码并确认登录";

                // Begin polling loop
                _ = Task.Run(async () =>
                {
                    while (!ct.IsCancellationRequested)
                    {
                        await Task.Delay(2000, ct);
                        if (ct.IsCancellationRequested) break;

                        var poll = await ApiClient.Instance.PollQrCodeAsync(qrInfo.Lp, ct);
                        if (poll != null && poll.Status == "success")
                        {
                            this.DispatcherQueue.TryEnqueue(() =>
                            {
                                LoginSuccess = true;
                                StatusHintText.Text = "✓ 登录成功！正在同步音箱设备...";
                                this.Hide();
                            });
                            break;
                        }
                    }
                }, ct);
            }
            else
            {
                StatusHintText.Text = $"获取二维码失败: {qrInfo?.Error ?? "未知错误"}";
                QrProgressRing.IsActive = false;
            }
        }
        catch (Exception ex)
        {
            StatusHintText.Text = $"异常: {ex.Message}";
            QrProgressRing.IsActive = false;
        }
    }

    private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        _pollCts?.Cancel();
        _pollCts = null;
    }
}
