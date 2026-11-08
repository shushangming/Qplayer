using System;
using System.IO;
using System.Security.Cryptography;

namespace KugouAvaloniaPlayer.Services;

/// <summary>
///     QQ 音乐登录 cookie 持久化服务。
///     使用 AppSqliteStore 存储，Windows 下用 ProtectedData 加密。
/// </summary>
public sealed class QqCookieService
{
    private const string ProtectedPrefix = "QQCOOKIE:v1:";
    private const string StoreScope = "qqmusic";
    private const string StoreKey = "cookie";

    private static readonly string LegacyPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "kugou",
        "qq_cookie.txt");

    /// <summary>
    ///     当前保存的 cookie（内存缓存）。
    /// </summary>
    public string? Cookie { get; private set; }

    public QqCookieService()
    {
        Cookie = Load();
    }

    /// <summary>
    ///     加载 cookie。
    /// </summary>
    public string? Load()
    {
        try
        {
            var content = AppSqliteStore.LoadValue(StoreScope, StoreKey);
            if (string.IsNullOrWhiteSpace(content))
            {
                if (!File.Exists(LegacyPath)) return null;
                content = File.ReadAllText(LegacyPath);
                AppSqliteStore.SaveValue(StoreScope, StoreKey, content);
                try { File.Delete(LegacyPath); } catch { /* ignore */ }
            }

            return Unprotect(content);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     保存 cookie。
    /// </summary>
    public void Save(string? cookie)
    {
        try
        {
            Cookie = cookie;
            if (string.IsNullOrWhiteSpace(cookie))
            {
                AppSqliteStore.DeleteValue(StoreScope, StoreKey);
                return;
            }

            var protectedContent = Protect(cookie);
            AppSqliteStore.SaveValue(StoreScope, StoreKey, protectedContent);
        }
        catch
        {
            // 忽略持久化失败，避免影响核心流程
        }
    }

    /// <summary>
    ///     清除 cookie。
    /// </summary>
    public void Clear()
    {
        Cookie = null;
        try
        {
            AppSqliteStore.DeleteValue(StoreScope, StoreKey);
            try { File.Delete(LegacyPath); } catch { /* ignore */ }
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    ///     是否已登录（有非空 cookie）。
    /// </summary>
    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Cookie);

    private static string Protect(string value)
    {
        if (!OperatingSystem.IsWindows()) return value;
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return ProtectedPrefix + Convert.ToBase64String(protectedBytes);
    }

    private static string? Unprotect(string content)
    {
        if (!content.StartsWith(ProtectedPrefix, StringComparison.Ordinal)) return content;
        if (!OperatingSystem.IsWindows()) return null;
        try
        {
            var protectedBytes = Convert.FromBase64String(content[ProtectedPrefix.Length..]);
            var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }
}
