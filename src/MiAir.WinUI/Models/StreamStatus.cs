using System;
using System.Text.Json.Serialization;

namespace MiAir.WinUI.Models;

public class StreamStatusResponse
{
    [JsonPropertyName("running")]
    public bool Running { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("source")]
    public SourceSnapshot Source { get; set; } = new();

    [JsonPropertyName("token")]
    public TokenStatus? Token { get; set; }

    [JsonPropertyName("config")]
    public ConfigInfo Config { get; set; } = new();
}

public class SourceSnapshot
{
    [JsonPropertyName("policy")]
    public string Policy { get; set; } = "latest";

    [JsonPropertyName("generation")]
    public ulong Generation { get; set; }

    [JsonPropertyName("active")]
    public ActiveSession? Active { get; set; }
}

public class ActiveSession
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = string.Empty; // "airplay" or "dlna"

    [JsonPropertyName("device")]
    public string Device { get; set; } = string.Empty;

    [JsonPropertyName("started_at")]
    public DateTime StartedAt { get; set; }

    [JsonPropertyName("last_activity")]
    public DateTime LastActivity { get; set; }
}

public class TokenStatus
{
    [JsonPropertyName("has_credentials")]
    public bool HasCredentials { get; set; }

    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    [JsonPropertyName("last_refresh")]
    public DateTime? LastRefresh { get; set; }

    [JsonPropertyName("last_error")]
    public string? LastError { get; set; }
}

public class ConfigInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "小爱音箱投放";

    [JsonPropertyName("target_did")]
    public string TargetDid { get; set; } = string.Empty;

    [JsonPropertyName("airplay_enabled")]
    public bool AirPlayEnabled { get; set; } = true;

    [JsonPropertyName("dlna_enabled")]
    public bool DlnaEnabled { get; set; } = true;

    [JsonPropertyName("buffer_ms")]
    public int BufferMs { get; set; } = 500;

    [JsonPropertyName("source_policy")]
    public string SourcePolicy { get; set; } = "latest";

    [JsonPropertyName("preferred_protocol")]
    public string PreferredProtocol { get; set; } = "airplay";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.1.2";
}
