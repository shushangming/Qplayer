namespace KgWebApi.Net.Services;

public interface IKgWebSessionContext
{
    string SessionKey { get; set; }
}

public sealed class KgWebSessionContext : IKgWebSessionContext
{
    public string SessionKey { get; set; } = KgWebSessionDefaults.FallbackSessionKey;
}

public static class KgWebSessionDefaults
{
    public const string HeaderName = "X-Kg-Session-Id";
    public const string CookieName = "kg_sid";
    public const string FallbackSessionKey = "default";
}
