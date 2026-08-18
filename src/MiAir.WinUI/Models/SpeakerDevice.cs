using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MiAir.WinUI.Models;

public class DeviceListResponse
{
    [JsonPropertyName("devices")]
    public List<SpeakerDevice> Devices { get; set; } = new();

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

public class SpeakerDevice
{
    [JsonPropertyName("deviceID")]
    public string DeviceID { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("hardware")]
    public string Hardware { get; set; } = string.Empty;

    [JsonPropertyName("alias")]
    public string Alias { get; set; } = string.Empty;

    [JsonPropertyName("currentLocalIP")]
    public string CurrentLocalIP { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsSelected { get; set; }

    [JsonIgnore]
    public string DisplayName => string.IsNullOrWhiteSpace(Alias) ? Name : $"{Name} ({Alias})";
}
