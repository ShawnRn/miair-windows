using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MiAir.WinUI.Models;

namespace MiAir.WinUI.Services;

public class ApiClient
{
    private static readonly Lazy<ApiClient> _instance = new(() => new ApiClient());
    public static ApiClient Instance => _instance.Value;

    private readonly HttpClient _httpClient;
    private int _port = 8302;

    public string BaseUrl => $"http://127.0.0.1:{_port}";

    private ApiClient()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public void SetPort(int port)
    {
        _port = port;
    }

    public async Task<bool> IsCoreReachableAsync(CancellationToken ct = default)
    {
        try
        {
            var res = await _httpClient.GetAsync($"{BaseUrl}/api/status", ct);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<StreamStatusResponse?> GetStatusAsync(CancellationToken ct = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<StreamStatusResponse>($"{BaseUrl}/api/status", ct);
        }
        catch
        {
            return null;
        }
    }

    public async Task<QrCodeInfo?> GetQrCodeAsync(CancellationToken ct = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<QrCodeInfo>($"{BaseUrl}/api/qr", ct);
        }
        catch (Exception ex)
        {
            return new QrCodeInfo { Error = ex.Message };
        }
    }

    public async Task<QrPollResponse?> PollQrCodeAsync(string lpUrl, CancellationToken ct = default)
    {
        try
        {
            var encodedLp = Uri.EscapeDataString(lpUrl);
            return await _httpClient.GetFromJsonAsync<QrPollResponse>($"{BaseUrl}/api/qr/poll?lp={encodedLp}", ct);
        }
        catch (Exception ex)
        {
            return new QrPollResponse { Status = "error", Error = ex.Message };
        }
    }

    public async Task<DeviceListResponse?> GetDevicesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<DeviceListResponse>($"{BaseUrl}/api/devices", ct);
        }
        catch (Exception ex)
        {
            return new DeviceListResponse { Error = ex.Message };
        }
    }

    public async Task<bool> BindSpeakerAsync(string did, CancellationToken ct = default)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/speaker/bind", new { did }, ct);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> PauseSpeakerAsync(string did, CancellationToken ct = default)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/speaker/pause", new { did }, ct);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SetVolumeAsync(string did, int volume, CancellationToken ct = default)
    {
        try
        {
            var res = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/speaker/volume", new { did, volume }, ct);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LogoutAccountAsync(CancellationToken ct = default)
    {
        try
        {
            var res = await _httpClient.PostAsync($"{BaseUrl}/api/account/logout", null, ct);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
