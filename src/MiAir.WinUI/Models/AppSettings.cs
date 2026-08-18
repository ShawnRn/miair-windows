using System;

namespace MiAir.WinUI.Models;

public class AppSettings
{
    public string DeviceName { get; set; } = "小爱音箱投放";
    public string SelectedDid { get; set; } = string.Empty;
    public string SelectedSpeakerName { get; set; } = string.Empty;

    public bool AirPlayEnabled { get; set; } = true;
    public int AirPlayPort { get; set; } = 5000;
    public int HttpPort { get; set; } = 8300;
    public int BufferMs { get; set; } = 500;

    public bool DlnaEnabled { get; set; } = true;
    public int DlnaPort { get; set; } = 8301;

    public string SourcePolicy { get; set; } = "latest"; // "latest", "lock", "idle", "priority"
    public int IdleTimeout { get; set; } = 10;
    public string PreferredProtocol { get; set; } = "airplay";

    public bool StartOnBoot { get; set; } = false;
    public bool MinimizeToTrayOnClose { get; set; } = true;
    public string AppTheme { get; set; } = "Default"; // "Default", "Light", "Dark"
}
