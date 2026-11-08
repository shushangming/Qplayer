namespace KgWebApi.Net.Data.Entities;

public sealed class KgSessionEntity
{
    public string SessionKey { get; set; } = string.Empty;
    public string UserId { get; set; } = "0";
    public string Token { get; set; } = string.Empty;
    public string VipType { get; set; } = "0";
    public string VipToken { get; set; } = string.Empty;
    public string Dfid { get; set; } = "-";
    public string Mid { get; set; } = "-";
    public string Uuid { get; set; } = "-";
    public string InstallDev { get; set; } = string.Empty;
    public string InstallMac { get; set; } = string.Empty;
    public string InstallGuid { get; set; } = string.Empty;
    public string? T1 { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
