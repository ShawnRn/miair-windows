using System.Text.Json.Serialization;

namespace MiAir.WinUI.Models;

public class QrCodeInfo
{
    [JsonPropertyName("qr")]
    public string Qr { get; set; } = string.Empty;

    [JsonPropertyName("lp")]
    public string Lp { get; set; } = string.Empty;

    [JsonPropertyName("timeout")]
    public int Timeout { get; set; } = 300;

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class QrPollResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "waiting"; // "waiting", "success", "timeout", "error"

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
